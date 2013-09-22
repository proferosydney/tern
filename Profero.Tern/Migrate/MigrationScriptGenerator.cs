using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Profero.Tern.Migrate
{
    public class MigrationScriptGenerator
    {
        public void Generate(IEnumerable<MigrationVersion> versions, IDatabaseProvider provider, TextWriter output, ScriptGenerationOptions options)
        {
            provider.CreateHeader(output);

            if (options.UseTransaction)
            {
                provider.BeginTransaction(output);
            }

            var versionTable = CreateVersionTable(options.VersionTableName);

            if (options.TrackVersions)
            {
                provider.CreateTable(options.VersionTableName, output);
            }

            var allVersions = GetTernMigrationVersions().Concat(versions);

            foreach (var version in allVersions)
            {
                //var versionEntry = CreateVersionRow(versionTable, version);

                //version.Hash = (string)versionEntry["Checksum"];

                provider.MigrateVersion(version, output, options);
            }

            if (options.UseTransaction)
            {
                provider.CommitTransaction(output);
            }
        }

        IEnumerable<MigrationVersion> GetTernMigrationVersions()
        {
            return new MigrationVersion[0];
        }

        static DataTable CreateVersionTable(string tableName)
        {
            var table = new DataTable(tableName);

            table.Columns.Add(new DataColumn("Schema", typeof(String)) { AllowDBNull = false, MaxLength = 32 });
            table.Columns.Add(new DataColumn("Version", typeof(String)) { AllowDBNull = false, MaxLength = 32 });
            table.Columns.Add(new DataColumn("Checksum", typeof(String)) { AllowDBNull = false, MaxLength = 32 });
            table.Columns.Add(new DataColumn("Description", typeof(String)) { AllowDBNull = true, MaxLength = Int32.MaxValue });
            table.Columns.Add(new DataColumn("DateAdded", typeof(DateTime)) { AllowDBNull = false });

            table.PrimaryKey = new [] { table.Columns["Version"] };

            return table;
        }
    }

    public class ScriptGenerationOptions
    {
        const string DefaultVersionTableName = "DatabaseVersion";

        public ScriptGenerationOptions()
        {
            VersionTableName = DefaultVersionTableName;
            UseTransaction = true;
            TrackVersions = true;
            ProcessBatchedScripts = true;
        }

        public bool UseTransaction { get; set; }

        public bool TrackVersions { get; set; }

        public string VersionTableName { get; set; }

        public bool ProcessBatchedScripts { get; set; }

        public string BatchTerminator { get; set; }
    }

    public class MigrationScriptOptions
    {
        public MigrationScriptOptions()
        {
            VersionFilenamePattern = DefaultVersionFilenamePattern;
            VersioningStyle = VersioningStyle.Version;
            DetectVersionInconsitencies = true;
        }

        private IVersionStrategy versionStrategy;
        public IVersionStrategy VersionStrategy
        {
            get
            {
                return versionStrategy;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                versionStrategy = value;
                VersioningStyle = Migrate.VersioningStyle.Custom;
            }
        }

        public bool DetectVersionInconsitencies { get; set; }

        private VersioningStyle versioningStyle;

        public VersioningStyle VersioningStyle
        {
            get { return versioningStyle; }
            set
            {
                versioningStyle = value;
                if (versioningStyle != Migrate.VersioningStyle.Custom)
                {
                    switch (versioningStyle)
                    {
                        case Migrate.VersioningStyle.Version:
                            versionStrategy = new VersionVersionStrategy();
                            break;
                        case Migrate.VersioningStyle.String:
                            versionStrategy = new TextVersionStrategy();
                            break;
                        case Migrate.VersioningStyle.Date:
                            versionStrategy = new DateVersionStrategy();
                            break;
                        default:
                            throw new ArgumentException();
                    }
                }
            }
        }

        public Regex VersionFilenamePattern { get; set; }

        public static readonly Regex DefaultVersionFilenamePattern = new Regex(@"^(?<version>(?:\d+[\.-])*\d+).*\.sql$");
        public const string VersionPatternGroupName = "version";
        public const string SchemaPatternGroupName = "schema";

        public static bool IsValidVersionFilenamePattern(Regex pattern)
        {
            if (pattern == null) throw new ArgumentNullException("pattern");

            string[] groupNames = pattern.GetGroupNames();

            return Array.IndexOf(groupNames, VersionPatternGroupName) != -1;
        }

        public void Validate()
        {
            if (DetectVersionInconsitencies && !VersionStrategy.SupportsNumericVersion)
            {
                throw new InvalidOperationException("DetectVersionInconsitencies must be false if the versioning style cannot be converted to a numeric value");
            }
        }
    }

    public class MigrationVersion
    {
        public MigrationVersion(string schema, string version, long numericVersion, string description, string skipCondition, string script)
            : this(schema, version, numericVersion, description, skipCondition, script, MigrationVersionFactory.GenerateChecksum(script))
        {
        }

        public MigrationVersion(string schema, string version, long numericVersion, string description, string skipCondition, string script, string checksum)
        {
            this.Version = version;
            this.NumericVersion = numericVersion;
            this.Description = description;
            this.SkipCondition = skipCondition;
            this.Script = script;
            this.Schema = schema;
            this.Checksum = checksum;
        }

        public string Version { get; private set; }

        public long NumericVersion { get; private set; }

        public string Description { get; private set; }

        public string SkipCondition { get; private set; }

        public string Script { get; private set; }

        public string Schema { get; private set; }

        public string Checksum { get; private set; }

        public static MigrationVersionFactory Factory
        {
            get { return new MigrationVersionFactory(); }
        }

        public  static IEnumerable<MigrationVersion> Sort(IEnumerable<MigrationVersion> versions, IVersionStrategy versionStrategy)
        {
            return (versionStrategy.SupportsNumericVersion)
                ? versions.OrderBy(x => x.NumericVersion)
                : versions.OrderBy(x => x.Version);
        }
    }

    public class MigrationVersionFactory
    {
        public bool CanCreate(string filename, MigrationScriptOptions options)
        {
            return options.VersionFilenamePattern.IsMatch(filename);
        }

        public MigrationVersion Create(TextReader reader, string filename, MigrationScriptOptions options, string defaultSchema = "default")
        {
            options.Validate();

            Match match = options.VersionFilenamePattern.Match(filename);

            if (!match.Success)
            {
                throw new ArgumentException("Filename does not match migration version pattern");
            }

            string version = GetVersion(match);
            string schema = GetSchema(match, defaultSchema);

            MigrationVersionScriptMetadata metadata;

            string scriptContents = ParseScriptContents(reader, out metadata);

            string checksum = GenerateChecksum(scriptContents);

            long numericVersion = (options.VersionStrategy.SupportsNumericVersion)
                ? options.VersionStrategy.GetNumericVersion(version)
                : 0L;

            return new MigrationVersion(schema, version, numericVersion, metadata.Description, metadata.SkipCondition, scriptContents, checksum);
        }

        string GetSchema(Match match, string defaultSchema)
        {
 	        var schemaGroup = match.Groups[MigrationScriptOptions.SchemaPatternGroupName];

            return (schemaGroup == null || !schemaGroup.Success)
                ? defaultSchema
                : schemaGroup.Value;
        }

        string GetVersion(Match match)
        {
            var versionGroup = match.Groups[MigrationScriptOptions.VersionPatternGroupName];

            if (versionGroup == null || !versionGroup.Success)
            {
                throw new ArgumentException("Filename did not specify a version");
            }

            return versionGroup.Value;
        }

        string ParseScriptContents(TextReader reader, out MigrationVersionScriptMetadata metadata)
        {
            StringBuilder scriptContentBuilder = new StringBuilder();

            string description = null;
            string skipCondition = null;

            metadata = new MigrationVersionScriptMetadata();

            foreach(string line in reader.GetLines())
            {
                if (line.StartsWith(CommentPrefix))
                {
                    if (metadata.SkipCondition == null && TryParseSkipCondition(line, out skipCondition))
                    {
                        metadata.SkipCondition = skipCondition;
                        continue;
                    }

                    if (metadata.Description == null && TryParseDescription(line, out description))
                    {
                        metadata.Description = description;
                        continue;
                    }
                }

                scriptContentBuilder.AppendLine(line);
                break;
            }

            scriptContentBuilder.Append(reader.ReadToEnd());

            return scriptContentBuilder.ToString();
        }

        bool TryParseDescription(string line, out string description)
        {
            description = line.Substring(CommentPrefix.Length).Trim();

            return true;
        }

        bool TryParseSkipCondition(string line, out string skipCondition)
        {
            line = line.Substring(CommentPrefix.Length).Trim();

            if (line.StartsWith(SkipLinePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                skipCondition = line.Substring(SkipLinePrefix.Length).Trim();
                return true;
            }

            skipCondition = null;
            return false;
        }

        const string SkipLinePrefix = "Skip:";
        const string CommentPrefix = "--";

        static MD5 md5 = MD5.Create();

        public static string GenerateChecksum(string scriptContent)
        {
            byte[] versionBytes = Encoding.UTF8.GetBytes(scriptContent.Trim());

            byte[] hashBytes = md5.ComputeHash(versionBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }

    class MigrationVersionScriptMetadata
    {
        public string Description { get; set; }
        public string SkipCondition { get; set; }
    }

    public enum VersioningStyle
    {
        Custom = 0,
        String = 1,
        Version = 2,
        Date = 3
    }
}
