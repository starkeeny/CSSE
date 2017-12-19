using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse
{
    public class Configuration
    {
        public bool LoadFromCommandLineIfEmpty { get; set; }
        public string LogFolder { get; set; }
        public string LogFilename { get; set; }
        public bool DebugLoggingEnabled { get; set; }
        public string ScriptFolder { get; set; }
        public string TemplateFile { get; set; }
        public string EnvironmentFolder { get; set; }
        public string BuildFolder { get; set; }
        public string EditorPath { get; set; }
        public string ArchiveFolder { get; set; }
    }
}
