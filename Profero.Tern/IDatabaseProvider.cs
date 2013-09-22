using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profero.Tern
{
    public interface IDatabaseProvider
    {
        void CreateHeader(System.IO.TextWriter output);

        void BeginTransaction(System.IO.TextWriter output);

        void CreateTable(string tableName, TextWriter output);

        void CommitTransaction(System.IO.TextWriter output);

        void MigrateVersion(MigrationVersion version, TextWriter output, ScriptGenerationOptions options);
    }
}
