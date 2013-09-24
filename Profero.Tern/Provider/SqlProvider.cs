using Profero.Tern.Migrate;
using Profero.Tern.Provider;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Profero.Tern.Provider
{
    public abstract class SqlScriptGenerator : IDatabaseScriptGenerator
    {
        public SqlScriptGenerator()
        {
        }

        public void Generate(IEnumerable<MigrationVersion> versions, TextWriter output, ScriptGenerationOptions options)
        {
            if (options.UseTransaction)
            {
                GenerateScriptForBeginTransaction(output);
            }

            if (options.TrackVersions)
            {
                GenerateScriptForVersionTrackingStorage(options.VersionTableName, output);
            }

            foreach (var version in versions)
            {
                if (options.TrackVersions)
                {
                    GenerateScriptForVersionContinuityChecks(version, options.VersionTableName, options.UseTransaction, output);
                }

                bool hasSkipCondition = !String.IsNullOrEmpty(version.SkipCondition);

                if (hasSkipCondition)
                {
                    GenerateScriptForSkipCondition(version, options, output);
                }

                GenerateScriptForApplyingVersionScript(version, options, output);
                GenerateScriptForDetectingFailedUpgrade(version, options, output);

                if (hasSkipCondition)
                {
                    output.WriteLine("END");
                }

                if (options.TrackVersions)
                {
                    GenerateScriptToTrackVersion(version, options, output);
                    output.WriteLine("END");
                }
            }

            if (options.UseTransaction)
            {
                GenerateScriptForCommitTransaction(output);
            }
        }

        protected virtual void GenerateScriptToTrackVersion(MigrationVersion version, ScriptGenerationOptions options, TextWriter output)
        {
            output.WriteLine(@"INSERT INTO [{0}] ([Schema], [Version], [NumericVersion], [Checksum], [Description], [DateAdded])
                VALUES (N'{1}', N'{2}', {3}, '{4}', N'{5}', getutcdate())",
                options.VersionTableName, Escape(version.Schema), Escape(version.Version), version.NumericVersion, Escape(version.Checksum), Escape(version.Description));
        }

        protected virtual void GenerateScriptForDetectingFailedUpgrade(MigrationVersion version, ScriptGenerationOptions options, TextWriter output)
        {
            output.WriteLine(@"IF @@ERROR <> 0 BEGIN");

            output.Write("PRINT N'Error occured while applying version {0} to schema {1}", Escape(version.Version), Escape(version.Schema));

            if (options.UseTransaction)
            {
                output.WriteLine(", rolling back transaction'");
                output.WriteLine("ROLLBACK TRANSACTION");
            }
            else
            {
                output.WriteLine("'");
            }

            output.WriteLine("RETURN");
            output.WriteLine("END");
        }

        protected virtual void GenerateScriptForApplyingVersionScript(MigrationVersion version, ScriptGenerationOptions options, TextWriter output)
        {
            output.WriteLine("PRINT N'Applying version {0} to schema {1}'", Escape(version.Version), Escape(version.Schema));

            if (options.ProcessBatchedScripts)
            {
                string[] scriptParts = GetScriptParts(version.Script, options.BatchTerminator);

                foreach (string scriptPart in scriptParts)
                {
                    output.WriteLine("exec sp_executesql N'{0}'", Escape(scriptPart));
                }
            }
            else
            {
                output.WriteLine(version.Script);
            }
        }

        protected virtual void GenerateScriptForVersionContinuityChecks(MigrationVersion version, string tableName, bool rollbackTransactionOnError, TextWriter output)
        {
            output.WriteLine(@"IF EXISTS(SELECT [Version] FROM [{0}] WHERE [Schema] = N'{1}' AND [Version] = N'{2}')
BEGIN
    IF NOT EXISTS(SELECT [Version] FROM [{0}] WHERE Version = N'{2}' AND Checksum = '{3}')
    BEGIN
        RAISERROR (N'Conflicting checksum while applying version %s to schema %s', 15, 0, N'{2}', N'{1}')
", tableName, Escape(version.Schema), Escape(version.Version), Escape(version.Checksum));

            if (rollbackTransactionOnError)
            {
                output.WriteLine("ROLLBACK TRANSACTION");
            }

            output.WriteLine(@"
        RETURN
    END
END
ELSE
BEGIN
    IF EXISTS(SELECT [Version] FROM [{0}] WHERE [Schema] = '{1}' AND [NumericVersion] > {3})
    BEGIN
        SELECT TOP 1 @Tern_ConflictingVersion = [Version] FROM [{0}] WHERE [Schema] = '{1}' AND [NumericVersion] > {3}
        RAISERROR (N'Cannot apply older version %s to schema %s when newer version %s exists', 15, 0, N'{2}', N'{1}', @Tern_ConflictingVersion)
        RETURN
    END

", tableName, Escape(version.Schema), Escape(version.Version), version.NumericVersion);
        }

        protected virtual void GenerateScriptForBeginTransaction(System.IO.TextWriter output)
        {
            output.WriteLine("BEGIN TRANSACTION");
        }

        protected virtual void GenerateScriptForTableExists(string tableName, bool exists, System.IO.TextWriter output)
        {
            if (!exists)
            {
                output.Write("NOT ");
            }

            output.WriteLine("EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')", Escape(tableName));
        }

        protected virtual void GenerateScriptForEndIf(System.IO.TextWriter output)
        {
            output.WriteLine("END");
        }

        protected virtual void GenerateScriptForVersionTrackingStorage(string tableName, System.IO.TextWriter output)
        {
            output.WriteLine("DECLARE @Tern_ConflictingVersion BIGINT");

            output.WriteLine("IF NOT EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')", Escape(tableName));
            output.WriteLine("BEGIN");

            output.WriteLine(@"CREATE TABLE [{0}]
(
    [Schema] NVARCHAR(32) NOT NULL,
    [Version] NVARCHAR(32) NOT NULL,
    [NumericVersion] BIGINT NOT NULL,
    [Checksum] NVARCHAR(32) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [DateAdded] DATETIME NOT NULL,
    CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([Schema], [Version])
)", tableName);

            output.WriteLine("END");
        }

        protected virtual void GenerateScriptForCommitTransaction(System.IO.TextWriter output)
        {
            output.WriteLine("COMMIT TRANSACTION");
        }

        protected virtual void GenerateScriptForSkipCondition(MigrationVersion version, ScriptGenerationOptions options, TextWriter output)
        {
            output.WriteLine(@"IF ({0})
BEGIN
    PRINT N'Skipping version {1} on schema {2} due to a matched skip condition'
END
ELSE
BEGIN
", version.SkipCondition, Escape(version.Version), Escape(version.Schema));
        }

        static string[] GetScriptParts(string script, string batchTerminator)
        {
            batchTerminator = batchTerminator ?? DefaultBatchTerminator;

            return Regex.Split(script, "^" + batchTerminator + "$", RegexOptions.None);
        }

        const string DefaultBatchTerminator = "GO";

        const int SqlUserErrorBaseCode = 50000;

        static string Escape(string value)
        {
            return value == null ? null : value.Replace("'", "''");
        }
    }
}
