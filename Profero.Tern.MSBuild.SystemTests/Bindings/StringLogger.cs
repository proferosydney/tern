using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace Profero.Tern.MSBuild.SystemTests.Bindings
{
    public class StringLogger : ILogger
    {
        IEventSource eventSource;

        readonly StringBuilder sb = new StringBuilder();

        public void Initialize(IEventSource eventSource)
        {
            this.eventSource = eventSource;

            eventSource.ErrorRaised += eventSource_ErrorRaised;
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            sb.AppendLine(e.Message);
        }

        public void Shutdown()
        {
            eventSource.ErrorRaised -= eventSource_ErrorRaised;
        }

        public LoggerVerbosity Verbosity { get; set; }
        public string Parameters { get; set; }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
