using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profero.Tern.Provider
{
    public interface IDatabaseScriptGenerator
    {
        void GenerateScriptForInitialization(System.IO.TextWriter output);

        void GenerateScriptForBeginTransaction(System.IO.TextWriter output);

        void GenerateScriptForVersionTrackingStorage(string tableName, TextWriter output);

        void GenerateScriptForCommitTransaction(System.IO.TextWriter output);

        void MigrateVersion(MigrationVersion version, TextWriter output, ScriptGenerationOptions options);
    }
}
