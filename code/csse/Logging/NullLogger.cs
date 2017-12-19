using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse.Logging
{
    public class NullLogger : ILogger
    {
        public void Write(Exception exc) { }
        public void Write(string format, params object[] args) { }
        public void WriteScripttName(string script) { }
        public string GetLogContent() { return String.Empty; }
        public void CleanLog() { }
    }
}
