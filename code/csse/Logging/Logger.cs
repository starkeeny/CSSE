using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse.Logging
{
    public class Logger : ILogger
    {
        private const string Ident = "  ";
        private readonly bool debugLoggingEnabled;
        private int MAXSIZE = (2 * 1024 * 1024); // 2 MB
        private string LogPath { get; set; }
        private string ArchLogPath { get; set; }

        public Logger(Configuration conf)
        {
            Directory.CreateDirectory(conf.LogFolder);

            this.LogPath = Path.Combine(conf.LogFolder, conf.LogFilename);
            this.ArchLogPath = Path.Combine(conf.LogFolder, conf.LogFilename) + ".000";
            this.debugLoggingEnabled = conf.DebugLoggingEnabled;

            var fileInfo = new FileInfo(LogPath);
            if (fileInfo.Exists)
            {
                if (fileInfo.Length > MAXSIZE)
                {
                    if (File.Exists(ArchLogPath))
                    {
                        File.Delete(ArchLogPath);
                    }
                    fileInfo.MoveTo(ArchLogPath);
                }
            }

        }

        public void WriteScripttName(string script) => Write("{1}project {0} selected", script, Ident);
        public void Write(Exception exc) => WriteInternal("{0}{1}{2}", new[] { exc.Message, Environment.NewLine, exc.StackTrace }, "ERR");
        public void Write(string format, params object[] args) => WriteInternal(format, args, "INFO");
        private void WriteInternal(string format, object[] args, string infoLevel)
        {
            string logText = string.Format("[{1:4}] {0}: ", DateTime.Now, infoLevel) + string.Format(format, args) + Environment.NewLine;

            if (debugLoggingEnabled)
            {
                var foregroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(logText);
                Console.ForegroundColor = foregroundColor;
            }

            File.AppendAllText(LogPath, logText);
        }

        public string GetLogContent() => File.ReadAllText(this.LogPath);

        public void CleanLog()
        {
            File.Delete(this.LogPath);
            File.Delete(this.ArchLogPath);
        } 
    }
}
