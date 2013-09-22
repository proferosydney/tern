using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Profero.Tern.SqlServer
{
    public class SqlServerProvider : IDatabaseProvider
    {
        public void CreateHeader(TextWriter output)
        {
            output.WriteLine("DECLARE @Tern_ConflictingVersion BIGINT");
        }

        public void BeginTransaction(System.IO.TextWriter output)
        {
            output.WriteLine("BEGIN TRANSACTION");
        }

        public void CreateTable(string tableName, System.IO.TextWriter output)
        {
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

        void CreateColumn(DataColumn column, TextWriter output)
        {
            output.Write("[{0}] {1} {2}",
                column.ColumnName,
                GetSqlType(column),
                column.AllowDBNull ? "NULL" : "NOT NULL");
        }

        private string GetSqlType(DataColumn column)
        {
            if (column.DataType == typeof(String))
            {
                return column.MaxLength == Int32.MaxValue
                    ? "NTEXT"
                    : "NVARCHAR(" + column.MaxLength + ")";
            }

            if (column.DataType == typeof(DateTime))
            {
                return "DATETIME";
            }

            throw new NotSupportedException(column.DataType.FullName);
        }

        public void CommitTransaction(System.IO.TextWriter output)
        {
            output.WriteLine("COMMIT TRANSACTION");
        }

        public void MigrateVersion(Migrate.MigrationVersion version, System.IO.TextWriter output, Migrate.ScriptGenerationOptions options)
        {
            if (options.TrackVersions)
            {
                output.WriteLine(@"IF EXISTS(SELECT [Version] FROM [{0}] WHERE [Schema] = N'{1}' AND [Version] = N'{2}')
BEGIN
    IF NOT EXISTS(SELECT [Version] FROM [{0}] WHERE Version = N'{2}' AND Checksum = '{3}')
    BEGIN
        RAISERROR (N'Conflicting checksum while applying version %s to schema %s', 15, 0, N'{2}', N'{1}')
", options.VersionTableName, Escape(version.Schema), Escape(version.Version), Escape(version.Checksum));

                if (options.UseTransaction)
                {
                    output.WriteLine("ROLLBACK TRANSACTION");
                }

                output.WriteLine(@"
    END
END
ELSE
BEGIN
    IF EXISTS(SELECT [Version] FROM [{0}] WHERE [Schema] = '{1}' AND [NumericVersion] > {3})
    BEGIN
        SELECT TOP 1 @Tern_ConflictingVersion = [Version] FROM [{0}] WHERE [Schema] = '{1}' AND [NumericVersion] > {3}
        RAISERROR (N'Cannot apply older version %s to schema %s when newer version %s exists', 15, 0, N'{2}', N'{1}', @Tern_ConflictingVersion)
    END

", options.VersionTableName, Escape(version.Schema), Escape(version.Version), version.NumericVersion);
            }

            bool hasSkipCondition = !String.IsNullOrEmpty(version.SkipCondition);

            if (hasSkipCondition)
            {
                output.WriteLine(@"IF ({0})
BEGIN
    PRINT N'Skipping version {1} on schema {2} due to a matched skip condition'
END
ELSE
BEGIN
", version.SkipCondition, Escape(version.Version), Escape(version.Schema));
            }

            output.WriteLine("PRINT N'Applying version {0} to schema {1}'", Escape(version.Version), Escape(version.Schema));

            if (options.ProcessBatchedScripts)
            {
                string[] scriptParts = GetScriptParts(version.Script, options.BatchTerminator);

                foreach(string scriptPart in scriptParts)
                {
                    output.WriteLine("exec sp_executesql N'{0}'", Escape(scriptPart));
                }
            }
            else
            {
                output.WriteLine(version.Script);
            }

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

            if (hasSkipCondition)
            {
                output.WriteLine("END");
            }

            if (options.TrackVersions)
            {
                output.WriteLine(@"INSERT INTO [{0}] ([Schema], [Version], [NumericVersion], [Checksum], [Description], [DateAdded])
                VALUES (N'{1}', N'{2}', {3}, '{4}', N'{5}', getutcdate())",
                    options.VersionTableName, Escape(version.Schema), Escape(version.Version), version.NumericVersion, Escape(version.Checksum), Escape(version.Description));
            }

            if (options.TrackVersions)
            {
                output.WriteLine("END");
            }
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
