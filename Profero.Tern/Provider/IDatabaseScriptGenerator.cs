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
        void Generate(IEnumerable<MigrationVersion> versions, TextWriter output, ScriptGenerationOptions options);
    }
}
