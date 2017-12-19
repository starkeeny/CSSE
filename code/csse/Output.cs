using csse.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse
{
    public class OutputHandler
    {
        public ILogger Logger { get; private set; }

        public OutputHandler(Configuration conf, ILogger logger)
        {
            this.Logger = logger;
        }

        public void Error(StreamReader reader)
        {
            string content = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(content))
            {
                Error(content);
            }
            reader.Close();
        }

        public void Write(StreamReader reader)
        {
            string content = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(content))
            {
                Write(content);
            }
            reader.Close();
        }

        public void WriteLogContent() => this.Write(Logger.GetLogContent());
        public void WriteDelimiterLine() => Write("".PadLeft(80, '-'));
        public void Write() => Write(string.Empty);
        public void Write(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
            this.Logger.Write("OUTPUT:" + format, args);
        }

        public void Error(string format, params object[] args)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Write(format, args);
            Console.ForegroundColor = foregroundColor;
        }

        public void WriteErrorWrongArgumentHandling()
        {
            this.Write();
            this.Error("wrong usage");
            this.WriteDelimiterLine();
            this.WriteHelp();
        }

        public void WriteHelp()
        {
            this.Write();
            this.Write("CSSE - C# Script Environment");
            this.Write();
            this.Write("... help             : prints this help output");
            this.Write("... cls              : clear screen");
            this.Write("... log              : lists the log entries");
            this.Write("... cleanlog         : cleans the log");
            this.Write("... exit             : closes interactive mode");
            this.Write();
            this.Write("... list             : lists the managed scripts");
            this.Write("... edit   SCRIPTNAME: edits the specified script");
            this.Write("... create SCRIPTNAME: creates the specified script (alias: edit)");
            this.Write("... start  SCRIPTNAME: starts the specified script");
            this.Write("... startAsREST SNAME: starts the specified script as");
            this.Write("                       http get REST method on port 4567");
            this.Write("... build  SCRIPTNAME: builds the specified script");
            this.Write("... clean  SCRIPTNAME: cleans the specified script");
            this.Write();
            this.Write("SCRIPTNAME: name of the script as shown in 'list' without extension");
        }
    }
}
