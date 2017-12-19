using csse.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse
{
    public class InputHandler
    {
        public string[] Arguments { get; private set; }
        public ILogger Logger { get; private set; }
        public bool IsInteractive { get; set; }

        public InputHandler(Configuration conf, ILogger logger, string[] arguments)
        {
            this.Arguments = arguments;
            this.Logger = logger;
            this.IsInteractive = (conf.LoadFromCommandLineIfEmpty && this.Arguments.Count() == 0);
        }

        public void LoadArgumentsFromCommandLine()
        {
            Console.Write($"C#SE {DateTime.Now.ToLongTimeString()} >");
            var line = Console.ReadLine();
            this.Arguments = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void InitCommandLine()
        {
            this.Arguments = "cls".Split(new string[] { " " }, StringSplitOptions.None);
        }

        public ArgumentHandlerSession CheckForArguments()
        {
            return new ArgumentHandlerSession(this);
        }

        public void EndInteractive()
        {
            if (!this.IsInteractive)
            {
                throw new InvalidOperationException("It is not allowed to close a non-interactive session");
            }

            this.IsInteractive = false;
        }
    }
}
