using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedAssembly
{
    public class Program
    {
        [DllImport("User32.dll", EntryPoint = "MessageBox",
            CharSet = CharSet.Auto)]
        internal static extern int MsgBox(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);


        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        private const int VK_1 = 0x31;
        private const int VK_2 = 0x32;

        public static int Main(string[] args)
        {
            Console.WriteLine("Main was called");

            string currentExe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            Console.WriteLine(currentExe);

            return 2;
        }

        public static int Run()
        {
            Console.WriteLine("Run was called");

            string currentExe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            string msg = "Process path: " + currentExe;
            Console.WriteLine(msg);
            MsgBox(IntPtr.Zero, msg, "From ManagedAssembly", 0);


            while (GetAsyncKeyState(VK_2) == 0)
            {
                if (GetAsyncKeyState(VK_1) > 0)
                {
                   msg = "1 pressed";

                    Console.WriteLine(msg);
                    MsgBox(IntPtr.Zero, msg, "From ManagedAssembly", 0);
                }

                Thread.Sleep(1000);
            }

            Console.WriteLine("Exiting Run");

            return 3;
        }
    }
}
