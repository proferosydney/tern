using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;

namespace Profero.Tern.Provider
{
    public static class DatabaseProviderFactory
    {
        public static IDatabaseScriptGenerator CreateScriptGenerator(string name)
        {
            return Create<IDatabaseScriptGenerator>(name, GetDefaultContainer());
        }

        static ExportProvider GetDefaultContainer()
        {
            return new CompositionContainer(GetDefaultCatalog());
        }

        static ComposablePartCatalog GetDefaultCatalog()
        {
            return new DirectoryCatalog("");
        }

        internal static T Create<T>(string name, ExportProvider exportProvider)
        {
            return exportProvider.GetExport<T>(name).Value;
        }
    }
}
