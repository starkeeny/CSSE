
using System;
using System.Collections;
using System.Text;
using System.IO;

public class FreestyleScriptPrecompiler
{
    static public bool Compile(ref string content, string scriptFile, bool isPrimaryScript, Hashtable context)
    {
        if (!isPrimaryScript)
            return false;

        var code = new StringBuilder(4096);
        code.AppendLine(@"
//Auto-generated file

using System;
using System.IO;
using System.Linq;
using System.Text;
");

        bool headerProcessed = false;

        string line;

        using (var sr = new StringReader(content))
            while ((line = sr.ReadLine()) != null)
            {
                if (!headerProcessed && (!line.StartsWith("//") && line.Trim() != "" && (!line.TrimStart().StartsWith("using "))))
                {
                    headerProcessed = true;
                    code.AppendLine(
@"

namespace ScriptNamespace
{
    public class ScriptClass
    {
        public static int Main(string[] args)
        {
            // args = new[] { ""-service"" };

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine(ScriptClass.ScriptLogic(args));
                }
                else if (args.Length == 1 && args[0].StartsWith(""-service""))
                {
                    using (var host = StartScriptLogicAsRest(args[0].Substring(""-service"".Length), () => ScriptLogic(args.Skip(1).ToArray())))
                    {
                        Console.ReadLine();
                        host.Close();
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        public static string ScriptLogic(string[] args)
        {
                    // <CODE from repo>
                    ");
                        }

                code.AppendLine(line);
            }

        code.AppendLine(
@"
             // </CODE from repo>

            return null;
        }

        public static System.Net.HttpListener StartScriptLogicAsRest(string serviceParameter, Func<object> func)
        {
            var listener = new System.Net.HttpListener();
            listener.Prefixes.Add(string.Format(""http://localhost:4567/{0}/"", System.Diagnostics.Process.GetCurrentProcess().ProcessName));
            listener.Start();

            while (true)
            {
                System.Net.HttpListenerContext context = listener.GetContext();
                System.Net.HttpListenerRequest request = context.Request;
                System.Net.HttpListenerResponse response = context.Response;

                string responseString = func().ToString();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // You must close the output stream.
                output.Close();
            }
            return listener;
        }
    }
}
");

        content = code.ToString();
        return true;
    }
}