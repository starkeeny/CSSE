using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csse
{
    public class ArgumentHandlerSession
    {
        private InputHandler ArgumentHandler { get; set; }
        public bool IsCalledOnce { private set; get; }

        public ArgumentHandlerSession(InputHandler argumentHandler)
        {
            this.ArgumentHandler = argumentHandler;
            IsCalledOnce = false;
        }

        private void CreateOSFolderForMethodToProvideTabbing(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                return;
            }

            // I know this is kind of crazy, but in the dos box you can get autocompletion by creating folders
            // with the names of the methods. It works only if you are currently in the folder and not using csse
            // ever the path variables, but anyway... I know this is bad and ugly but it makes my manual testing so 
            // much easier that I will live with it. This line is only for dos autocompletion and has no other value.                
            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), method));
        }

        private ArgumentHandlerSession IsImplementation(string method, Action action, int countOfParametersMin, int countOfParametersMax)
        {
            CreateOSFolderForMethodToProvideTabbing(method);

            if (!IsCalledOnce && CheckArgumentCount(method, countOfParametersMin, countOfParametersMax))
            {
                this.IsCalledOnce = true;

                ArgumentHandler.Logger.Write("start command {0}", method);
                action();
                ArgumentHandler.Logger.Write("stop command {0}", method);
            }
            return this;
        }

        public ArgumentHandlerSession Is(string method, Func<string, string, Task> func, bool lastIsOptional = false)
            => IsImplementation(method, () =>
                func(ArgumentHandler.Arguments[1],
                    !lastIsOptional
                        ? ArgumentHandler.Arguments[2]
                        : (
                            ArgumentHandler.Arguments.Length == 2
                                ? string.Empty
                                : ArgumentHandler.Arguments[3]
                        )), lastIsOptional ? 2 : 3, 3);

        public ArgumentHandlerSession Is(string method, Action<string, string> action, bool lastIsOptional = false)
            => IsImplementation(method, () => 
                action(ArgumentHandler.Arguments[1], 
                    !lastIsOptional
                        ?ArgumentHandler.Arguments[2]
                        : (
                            ArgumentHandler.Arguments.Length == 2 
                            ? string.Empty 
                            : ArgumentHandler.Arguments[3]
                    )), lastIsOptional ? 2 :3, 3);

        public ArgumentHandlerSession Is(string method, Action<string> action)
            => IsImplementation(method, () => action(ArgumentHandler.Arguments[1]), 2, 2);

        public ArgumentHandlerSession Is(string method, Action action)
            => IsImplementation(method, action, 1, 1);

        public ArgumentHandlerSession Is(string method)
            => IsImplementation(method, () => { }, 0, 0);

        public ArgumentHandlerSession Else(Action action)
        {
            if (!IsCalledOnce)
            {
                this.IsCalledOnce = true;

                action();
            }

            return this;
        }

        private bool CheckArgumentCount(string method, int countArgsToCheckMin = 1, int countArgsToCheckMax = 1)
        {
            return  
                (ArgumentHandler.Arguments.Count() >= countArgsToCheckMin  && 
                 ArgumentHandler.Arguments.Count() <= countArgsToCheckMax) &&
                (ArgumentHandler.Arguments.Count() == 0 ||
                 string.Equals(ArgumentHandler.Arguments[0], method, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
