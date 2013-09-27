using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Profero.Tern.Migrate;

namespace Profero.Tern.MSBuild
{
    public class FindMigrationVersions : Task
    {
        public FindMigrationVersions()
        {
            Options = new MigrationScriptOptions();
        }

        [Required]
        public ITaskItem[] Directories { get; set; }

        [Output]
        public ITaskItem[] ScriptItems { get; set; }

        public MigrationScriptOptions Options { get; private set; }

        public string VersioningStyle
        {
            get { return Options.VersioningStyle.ToString(); }
            set { Options.VersioningStyle = (VersioningStyle)Enum.Parse(typeof(VersioningStyle), value); }
        }

        public override bool Execute()
        {
            var scriptFiles = Directories
                .Select(i => i.GetMetadata("FullPath"))
                .Select(p => new DirectoryInfo(p))
                .SelectMany(d => d.GetFiles("*.sql"));

            ScriptItems = GetValidScriptFiles(scriptFiles)
                .Select(CreateScriptItem)
                .ToArray();

            return true;
        }

        private ITaskItem CreateScriptItem(FileInfo arg)
        {
            var item = new TaskItem(arg.FullName);

            item.SetMetadata("Schema", arg.Directory.Name);

            return item;
        }

        private IEnumerable<FileInfo> GetValidScriptFiles(IEnumerable<FileInfo> scriptFiles)
        {
            var factory = MigrationVersion.Factory;

            foreach (var file in scriptFiles)
            {
                if (factory.CanCreate(file.FullName, Options))
                {
                    yield return file;
                }
                else
                {
                    Log.LogWarning("Skipping script '{0}' as it does not match the versioning strategy", file.Name);
                }
            }
        }
    }
}
