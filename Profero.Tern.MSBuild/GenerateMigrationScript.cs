using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Profero.Tern.Migrate;
using Profero.Tern.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Profero.Tern.MSBuild
{
    /// <summary>
    /// Generates a migration script from <see cref="M:Versions"/> into <see cref="O:Output"/>
    /// </summary>
    public class GenerateMigrationScript : Task
    {
        public GenerateMigrationScript()
        {
            MigrationScriptOptions = new MigrationScriptOptions();
            ScriptGenerationOptions = new ScriptGenerationOptions();
        }

        /// <summary>Gets or sets the version items</summary>
        /// <remarks>Each item should be a .sql file</remarks>
        [Required]
        public ITaskItem[] Versions { get; set; }

        public VersioningStyle VersioningStyle
        {
            get { return MigrationScriptOptions.VersioningStyle; }
            set { MigrationScriptOptions.VersioningStyle = value; }
        }

        [Required]
        public MigrationScriptOptions MigrationScriptOptions { get; private set; }

        [Required]
        public ScriptGenerationOptions ScriptGenerationOptions { get; private set; }

        public string DatabaseProvider { get; set; }

        public ITaskItem Output { get; set; }

        public override bool Execute()
        {
            IDatabaseScriptGenerator databaseProvider;

            if (!TryGetDatabaseProvider(DatabaseProvider, out databaseProvider))
            {
                return false;
            }

            var migrationVersions = GetMigrationVersions(Versions);

            

            string outputPath = Output.GetMetadata("FullPath");
            
            try
            {
                using (TextWriter writer = File.CreateText(outputPath))
                    databaseProvider.Generate(migrationVersions, writer, ScriptGenerationOptions);

                return true;
            }
            catch (IOException ex)
            {
                Log.LogError("Could not access output file '{0}': {1}", outputPath, ex.Message);
                return false;
            }
        }

        private bool TryGetDatabaseProvider(string name, out IDatabaseScriptGenerator scriptGenerator)
        {
            scriptGenerator = null;

            try
            {
                scriptGenerator = DatabaseProviderFactory.CreateScriptGenerator(name);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError("Could not resolve database provider '{0}': {1}", name, ex.Message);
                return false;
            }
        }

        private List<MigrationVersion> GetMigrationVersions(IEnumerable<ITaskItem> items)
        {
            List<MigrationVersion> output = new List<MigrationVersion>();

            var factory = MigrationVersion.Factory;

            foreach (var item in Versions)
            {
                string file = item.GetMetadata("FullPath");
                string schema = item.GetMetadata("Schema");

                FileInfo fileInfo = new FileInfo(file);

                if (!fileInfo.Exists)
                {
                    Log.LogWarning("Skipping version script '{0}' because it does not exist", item.ItemSpec);
                    continue;
                }

                if (!factory.CanCreate(fileInfo.FullName, MigrationScriptOptions))
                {
                    Log.LogWarning("Skipping version script '{0}' because it does not match the versioning pattern", item.ItemSpec);
                    continue;
                }

                using (var reader = fileInfo.OpenText())
                    output.Add(factory.Create(reader, fileInfo.FullName, MigrationScriptOptions, schema));
            }

            return output;
        }
    }
}
