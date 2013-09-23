using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Profero.Tern.Provider
{
    public interface IDatabaseProvider
    {
        IDatabaseScriptGenerator CreateGenerator();
    }
}
