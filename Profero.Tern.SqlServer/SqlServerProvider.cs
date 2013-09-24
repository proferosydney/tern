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

namespace Profero.Tern.SqlServer
{
    public class SqlServerProvider : IDatabaseProvider
    {
        public IDatabaseScriptGenerator CreateGenerator()
        {
            return new SqlServerScriptGenerator();
        }
    }

    [Export(SqlServerScriptGenerator.Name, typeof(IDatabaseScriptGenerator))]
    public class SqlServerScriptGenerator : SqlScriptGenerator
    {
        public const string Name = "SqlServer2005";
    }
}
