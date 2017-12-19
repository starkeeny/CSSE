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
    public class ScriptHandler
    {
        public ILogger Logger { get; private set; }
        public OutputHandler Output { get; private set; }

        public string ScriptFolder { get; private set; }
        public string BuildFolder { get; private set; }
        public string ArchiveFolder { get; private set; }
        public string EnvironmentFolder { get; private set; }
        public string TemplateFile { get; private set; }
        public string EditorPath { get; set; }

        public ScriptHandler(Configuration conf, ILogger logger, OutputHandler output)
        {
            this.ScriptFolder = conf.ScriptFolder;
            this.BuildFolder = conf.BuildFolder;
            this.ArchiveFolder = conf.ArchiveFolder;
            this.EnvironmentFolder = conf.EnvironmentFolder;

            this.TemplateFile = conf.TemplateFile;
            this.EditorPath = conf.EditorPath;

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

        public string GetScriptPath(string script) => Path.Combine(ScriptFolder, script + ".cs");
        public string GetBuildPath(string script) => Path.Combine(BuildFolder, script + ".exe");

        public void EditScript(string script)
        {
            this.Logger.WriteScripttName(script);

            var scriptPath = GetScriptPath(script);
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
            StartProcess(GetBuildPath(script), "");
        }
        public Task StartScriptAsREST(string script, string serviceParameter)
        {
            this.Logger.WriteScripttName(script);
            BuildScript(script);
            return StartProcessAsync(GetBuildPath(script), "-service" + serviceParameter);
        }

        public void BuildScript(string script)
        {
            this.Logger.WriteScripttName(script);
            StartProcess(Path.Combine(EnvironmentFolder, "cscs"), $"/e {Path.Combine(ScriptFolder, script + ".cs")} /r:System.Net.Http");

            File.Delete(Path.Combine(BuildFolder, script + ".exe"));
            File.Move(Path.Combine(ScriptFolder, script + ".exe"), Path.Combine(BuildFolder, script + ".exe"));
        }

        public void CleanScript(string script)
        {
            this.Logger.WriteScripttName(script);
            File.Move(Path.Combine(ScriptFolder, script + ".cs"), Path.Combine(ArchiveFolder, script + ".cs" + DateTime.Now.Ticks.ToString()));
            File.Delete(Path.Combine(BuildFolder, script + ".exe"));
        }

        private Task StartProcessAsync(string filename, string arguments)
            => Task.Run(() => StartProcess(filename, arguments));

        private void StartProcess(string filename, string arguments)
        {
            var info = new ProcessStartInfo(filename, arguments)
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            var process = Process.Start(info);
            process.WaitForExit();

            Output.Write(process.StandardOutput);
            Output.Error(process.StandardError);

            this.Logger.Write("Exited with {0}", process.ExitCode);
        }
    }
}
