using System;
using HookSpace;
using System.Windows.Forms;

namespace HookConsole
{
    class Program
    {
        public static int count = 0;

        public static void writeshit()
        {
            count++;
            Console.WriteLine("event happened " + count);
        }

        static void Main(string[] args)
        {
            globalKeyboardHook gkh = new globalKeyboardHook();
            gkh.KeyDown += writeshit;

            Application.Run();
        }
    }
}
