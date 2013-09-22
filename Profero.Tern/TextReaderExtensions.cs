using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profero.Tern
{
    internal static class TextReaderExtensions
    {
        /// <summary>
        /// Simple wrapper to provide a more function, and less stateful, enumeration of a TextReader's lines
        /// </summary>
        public static IEnumerable<string> GetLines(this TextReader source)
        {
            string line = source.ReadLine();

            while (line != null)
            {
                yield return line;

                line = source.ReadLine();
            }
        }
    }
}
