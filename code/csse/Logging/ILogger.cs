using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse.Logging
{
    public interface ILogger
    {
        void Write(Exception exc);
        void Write(string format, params object[] args);
        void WriteScripttName(string script);
        string GetLogContent();
        void CleanLog();
    }
}
