using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profero.Tern.UnitTests
{
    public class TestVersionScript
    {
        public TestVersionScript(string filename, string description, string skip, string script)
        {
            this.Filename = filename;
            this.Description = description;
            this.Skip = skip;
            this.Script = script;
        }

        public string Filename { get; private set; }
        public string Description { get; private set; }
        public string Script { get; private set; }
        public string Skip { get; private set; }

        public TextReader CreateReader()
        {
            return new StringReader(ToString());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if (!String.IsNullOrEmpty(Description))
            {
                builder.AppendFormat("-- {0}", Description);
                builder.AppendLine();
            }

            if (!String.IsNullOrEmpty(Skip))
            {
                builder.AppendFormat("-- Skip: {0}", Skip);
                builder.AppendLine();
            }

            builder.Append(Script);

            return builder.ToString();
        }

    }
}
