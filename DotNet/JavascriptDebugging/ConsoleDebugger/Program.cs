using System.Threading.Tasks;
using ConsoleDebugger.Debuggers;

namespace ConsoleDebugger
{
    public class Program
    {
        public static async Task Main()
        {
            //await ClearScriptDebugger.RunDebugExample();

            await new CefSharpDebugger().StartAsync();
        }
    }
}
