//------------------------------------------------------
// Copyright (c) 2016 - Daniel Kienböck / MIT license
// This file is part of the CSSE project.
//------------------------------------------------------

using csse.Logging;
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
        static void Main(string[] args)
        {
            var conf = new Configuration()
            {
                LoadFromCommandLineIfEmpty = true,
                LogFolder = "./_/log/",
                LogFilename = "csse.log",
                DebugLoggingEnabled = false,
                ScriptFolder = "./_/code/",
                TemplateFile = "./_/template/template.cs",
                EnvironmentFolder = "./_/environment/",
                BuildFolder = "./_/build/",
                EditorPath = "notepad",
                ArchiveFolder = "./_/archive/"
            };

            ILogger logger = LoggerFactory.Create(conf);

            var outputHandler = new OutputHandler(conf, logger);
            var scriptHandler = new ScriptHandler(conf, logger, outputHandler);
            var inputHandler = new InputHandler(conf, logger, args);
            bool firstCallInteractive = inputHandler.IsInteractive;

            List<Task> tasks = new List<Task>();

            logger.Write("starting csse");

            do
            {
                try
                {
                    if (firstCallInteractive)
                    {
                        inputHandler.InitCommandLine();
                        firstCallInteractive = false;
                    }
                    else if (inputHandler.IsInteractive)
                    {
                        inputHandler.LoadArgumentsFromCommandLine();
                    }

                    inputHandler.CheckForArguments()
                        .Is("")
                        .Is("exit", inputHandler.EndInteractive)
                        .Is("help", outputHandler.WriteHelp)
                        .Is("cleanlog", logger.CleanLog)
                        .Is("log", outputHandler.WriteLogContent)
                        .Is("list", scriptHandler.WriteList)
                        .Is("create", scriptHandler.EditScript)
                        .Is("edit", scriptHandler.EditScript)
                        .Is("type", script => outputHandler.Write(File.ReadAllText(scriptHandler.GetScriptPath(script))))
                        .Is("cls", () =>
                        {
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                        })
                        .Is("start", scriptHandler.StartScript)
                        .Is("startAsREST", (script, serviceParameter) => tasks.Add(scriptHandler.StartScriptAsREST(script, serviceParameter)), lastIsOptional: true)
                        .Is("build", scriptHandler.BuildScript)
                        .Is("clean", scriptHandler.CleanScript)

                        .Else(outputHandler.WriteErrorWrongArgumentHandling);
                }
                catch (Exception exc)
                {
                    logger.Write(exc);
                }

            } while (inputHandler.IsInteractive);

            logger.Write("stopping csse");
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press <Enter> to leave debug session");
                Console.ReadLine();
            }
        }
    }
}
