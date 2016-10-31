//------------------------------------------------------
// Copyright (c) 2016 - Daniel Kienböck / MIT license
// This file is part of the CSSE project.
//------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse
{
    class Program
    {
        public class ArgumentHandler
        {
            public string[] Arguments { get; private set; }
            public Logger Logger { get; private set; }
            public ArgumentHandler(string[] arguments, Logger logger, bool loadFromCommandLineIfEmpty)
            {
                this.Arguments = arguments;
                this.Logger = logger;
                if(loadFromCommandLineIfEmpty && this.Arguments.Count() == 0)
                {
                    LoadArgumentsFromCommandLine();
                }
            }

            private void LoadArgumentsFromCommandLine()
            {
                Console.Write("Args>");
                var line = Console.ReadLine();
                this.Arguments = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }

            public bool IsMethod(string method, int? countArgsToCheckMin = 1, int? countArgsToCheckMax = null)
            {
                // I know this is kind of crazy, but in the dos box you can get autocompletion by creating folders
                // with the names of the methods. It works only if you are currently in the folder and not using csse
                // ever the path variables, but anyway... I know this is bad and ugly but it makes my manual testing so 
                // much easier that I will live with it. This line is only for dos autocompletion and has no other value.                
                Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), method));

                int minArgs = countArgsToCheckMin ?? 1;
                int maxArgs = countArgsToCheckMax ?? (countArgsToCheckMin ?? 1);

                bool returnValue = (Arguments.Count() >= minArgs && Arguments.Count() <= maxArgs) &&
                                    string.Equals(Arguments[0], method, StringComparison.InvariantCultureIgnoreCase);

                this.Logger.ConditionalWrite(returnValue, "method '{0}' called...", method);

                return returnValue;
            }
            public bool IsMethod(string method, ref string param1)
            {
                bool returnValue = IsMethod(method, 2, 2);
                param1 = returnValue ? Arguments[1] : default(string);
                this.Logger.ConditionalWrite(returnValue, "   parameter '{0}' called...", param1);
                return returnValue;
            }
        }
        public class ScriptHandler
        {
            public Logger Logger { get; private set; }
            public Output Output { get; private set; }

            public string ScriptFolder { get; private set; }
            public string BuildFolder { get; private set; }
            public string ArchiveFolder { get; private set; }
            public string EnvironmentFolder { get; private set; }
            public string TemplateFile { get; private set; }
            public string EditorPath { get; set; }

            public ScriptHandler(string scriptFolder, string templateFile, string buildFolder, string editorPath, string archiveFolder, string environmentFolder, Logger logger, Output output)
            {
                this.ScriptFolder = scriptFolder;
                this.BuildFolder = buildFolder;
                this.ArchiveFolder = archiveFolder;
                this.EnvironmentFolder = environmentFolder;

                this.TemplateFile = templateFile;
                this.EditorPath = editorPath;

                this.Logger = logger;
                this.Output = output;

                Directory.CreateDirectory(this.ScriptFolder);
                Directory.CreateDirectory(this.BuildFolder);
                Directory.CreateDirectory(this.ArchiveFolder);
                Directory.CreateDirectory(this.EnvironmentFolder);
            }

            public void WriteList()
            {
                Directory.EnumerateFiles(ScriptFolder)
                         .ToList()
                         .ForEach((script) => Output.Write(" - {0}", Path.GetFileNameWithoutExtension(script)));
            }

            public void EditScript(string script)
            {
                this.Logger.WriteScripttName(script);

                string scriptPath = Path.Combine(ScriptFolder, script + ".cs");
                if (!File.Exists(scriptPath))
                {
                    if (File.Exists(TemplateFile))
                    {
                        File.Copy(TemplateFile, scriptPath);
                    }
                    else
                    {
                        File.AppendAllText(scriptPath, "");
                    }
                }

                StartProcess(EditorPath, scriptPath);
            }

            public void StartScript(string script)
            {
                this.Logger.WriteScripttName(script);
                BuildScript(script);
                StartProcess(Path.Combine(BuildFolder, script + ".exe"), "");
            }

            public void BuildScript(string script)
            {
                this.Logger.WriteScripttName(script);
                StartProcess(Path.Combine(EnvironmentFolder, "cscs"), string.Format("/e {0}", Path.Combine(ScriptFolder, script + ".cs")));

                File.Delete(Path.Combine(BuildFolder, script + ".exe"));
                File.Move(Path.Combine(ScriptFolder, script + ".exe"), Path.Combine(BuildFolder, script + ".exe"));
            }

            public void CleanScript(string script)
            {
                this.Logger.WriteScripttName(script);
                File.Move(Path.Combine(ScriptFolder, script + ".cs"), Path.Combine(ArchiveFolder, script + ".cs" + DateTime.Now.Ticks.ToString()));
                File.Delete(Path.Combine(BuildFolder, script + ".exe"));
            }

            private void StartProcess(string filename, string arguments)
            {
                var info = new ProcessStartInfo(filename, arguments);

                info.UseShellExecute = false;
                info.ErrorDialog = false;
                info.CreateNoWindow = true;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;

                var process = Process.Start(info);
                process.WaitForExit();

                Output.Write(process.StandardOutput);
                Output.Error(process.StandardError);

                this.Logger.Write("Exited with {0}", process.ExitCode);
            }
        }

        public class Logger
        {
            private const string IDENT = "  ";
            private readonly bool debugLoggingEnabled;
            private int MAXSIZE = (2 * 1024 * 1024); // 2 MB
            public string LogPath { get; set; }
            public string ArchLogPath { get; set; }

            public Logger(string logFolder, bool debugLoggingEnabled = false)
            {
                LogPath = Path.Combine(logFolder, "csse.log");
                ArchLogPath = Path.Combine(logFolder, "csse.log") + ".000";

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
                Directory.CreateDirectory(logFolder);
                this.debugLoggingEnabled = debugLoggingEnabled;
            }

            public void ConditionalWrite(bool condition, string format, params object[] args)
            {
                if (condition) Write(format, args);
            }

            public void Write(Exception exc)
            {
                WriteInternal("{0}{1}{2}", new[] { exc.Message, Environment.NewLine, exc.StackTrace }, "ERR");
            }

            public void Write(string format, params object[] args)
            {
                WriteInternal(format, args, "INFO");
            }

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

            public void WriteScripttName(string script)
            {
                Write("{1}project {0} selected", script, IDENT);
            }

            public string GetLogContent()
            {
                return File.ReadAllText(this.LogPath);
            }

            public void CleanLog()
            {
                File.Delete(this.LogPath);
            }
        }

        public class Output
        {
            public Logger Logger { get; private set; }

            public Output(Logger logger)
            {
                this.Logger = logger;
            }

            public void Error()
            {
                Error(string.Empty);
            }

            public void Error(Stream stream)
            {
                Error(new StreamReader(stream));
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

            public void Error(string format, params object[] args)
            {
                var foregroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Write(format, args);
                Console.ForegroundColor = foregroundColor;
            }

            public void Write()
            {
                Write(string.Empty);
            }

            public void Write(Stream stream)
            {
                Write(new StreamReader(stream));
            }

            public void Write(StreamReader reader)
            {
                Write(reader.ReadToEnd());
                reader.Close();
            }

            public void Write(string format, params object[] args)
            {
                Console.WriteLine(string.Format(format, args));
                this.Logger.Write("OUTPUT:" + format, args);
            }
            public void WriteDelimiterLine()
            {
                Write("".PadLeft(80, '-'));
            }
        }

        static void Main(string[] args)
        {
            string script = null;
            bool executedOnce = false;

            var logger = new Logger("./_/log/");
            var output = new Output(logger);
            var argumentHandler = new ArgumentHandler(args, logger, loadFromCommandLineIfEmpty: true);
            var scriptHandler = new ScriptHandler(scriptFolder: "./_/code/",
                                                  templateFile: "./_/template/template.cs",
                                                  environmentFolder: "./_/environment/",
                                                  buildFolder: "./_/build/",
                                                  editorPath: "notepad",
                                                  archiveFolder: "./_/archive/",

                                                  logger: logger,
                                                  output: output);


            Action<bool, Action, string> executeIf = (condition, code, logText) =>
            {
                if (condition)
                {
                    executedOnce = true;
                    logger.Write("start {0}", logText);
                    try
                    {
                        code();
                    }
                    catch (Exception exc)
                    {
                        logger.Write(exc);
                    }
                    logger.Write("stop  {0}", logText);
                }
            };

            logger.Write("starting csse");

            executeIf(argumentHandler.IsMethod("help"), () => WriteHelp(output), "print help");
            executeIf(argumentHandler.IsMethod("list"), () => scriptHandler.WriteList(), "list available scripts");
            executeIf(argumentHandler.IsMethod("cleanlog"), () => logger.CleanLog(), "clean log");
            executeIf(argumentHandler.IsMethod("log"), () => output.Write(logger.GetLogContent()), "print log");
            executeIf(argumentHandler.IsMethod("create", ref script), () => scriptHandler.EditScript(script), "edit scripts");
            executeIf(argumentHandler.IsMethod("edit", ref script), () => scriptHandler.EditScript(script), "edit scripts");
            executeIf(argumentHandler.IsMethod("start", ref script), () => scriptHandler.StartScript(script), "start scripts");
            executeIf(argumentHandler.IsMethod("build", ref script), () => scriptHandler.BuildScript(script), "build scripts");
            executeIf(argumentHandler.IsMethod("clean", ref script), () => scriptHandler.CleanScript(script), "clean scripts");

            executeIf(!executedOnce, () => WriteError(output), "print error because no argument was selected");

            logger.Write("stopping csse");
        }

        static void WriteError(Output output)
        {
            output.Write();
            output.Error("wrong usage");
            output.WriteDelimiterLine();
            WriteHelp(output);
        }

        static void WriteHelp(Output output)
        {
            output.Write();
            output.Write("CSSE - C# Script Environment");
            output.Write();
            output.Write("... help:              prints this help output");
            output.Write("... list:              lists the managed scripts");
            output.Write("... log:               lists the log entries");
            output.Write("... cleanlog:          cleans the log");
            output.Write("... edit  SCRIPTNAME:  edits the specified script");
            output.Write("... create SCRIPTNAME: creates the specified script (alias to edit)");
            output.Write("... start SCRIPTNAME:  starts the specified script");
            output.Write("... build SCRIPTNAME:  builds the specified script");
            output.Write("... clean SCRIPTNAME:  cleans the specified script");
            output.Write();
            output.Write("SCRIPTNAME: name of the script as shown in 'list' without extension");
        }
    }
}
