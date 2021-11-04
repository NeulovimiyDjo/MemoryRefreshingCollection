using System;
using System.Runtime.InteropServices;

namespace InjectorNetCoreConsole
{
    class Program
    {
        const string InjectorDllPath = "../../../../x64/Release/InjectorDll.dll";
        const string DllHidePath = "../../../../x64/Release/DllHide.dll";


        [DllImport(InjectorDllPath)]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern uint GetProcessIdWrapper(
            [param: MarshalAs(UnmanagedType.LPStr)]string targetProcessName
        );

        [DllImport(InjectorDllPath)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LoadLibraryInjectWrapper(
            [param: MarshalAs(UnmanagedType.U4)] uint targetProcessID,
            [param: MarshalAs(UnmanagedType.LPStr)] string dllFullName
        );

        [DllImport(InjectorDllPath)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ManualMapInjectWrapper(
            [param: MarshalAs(UnmanagedType.U4)] uint targetProcessID,
            [param: MarshalAs(UnmanagedType.LPStr)] string dllFullName
        );


        static void Main(string[] args)
        {
            var ID = GetProcessIdWrapper("Taskmgr.exe");

            bool success = ManualMapInjectWrapper(ID, DllHidePath);

            Console.WriteLine(ID);
            Console.WriteLine(success);
        }
    }
}
