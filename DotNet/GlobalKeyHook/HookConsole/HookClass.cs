using System;
using System.Runtime.InteropServices;

namespace HookSpace
{
    class globalKeyboardHook
    {
        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);
        public delegate void EventHandler();

        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;

        IntPtr hhook = IntPtr.Zero;
        IntPtr hInstance = LoadLibrary("User32");
        private keyboardHookProc hookProcDelegate;
        public event EventHandler KeyDown;
        public event EventHandler KeyUp;



        public globalKeyboardHook()
        {
            hookProcDelegate = hookProc;
            hook();
        }

        ~globalKeyboardHook()
        {
            unhook();
        }
      

        public void hook()
        {
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProcDelegate, hInstance, 0);
        }

        public void unhook()
        {
            UnhookWindowsHookEx(hhook);
        }


        public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                {
                    KeyDown();
                    Console.WriteLine(code + " " + wParam + " " + lParam.vkCode + " " + lParam.scanCode + " " + lParam.flags + " " + lParam.time + " " + lParam.dwExtraInfo);
                }
                else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                {
                    KeyUp();
                    Console.WriteLine(code + " " + wParam + " " + lParam.vkCode + " " + lParam.scanCode + " " + lParam.flags + " " + lParam.time + " " + lParam.dwExtraInfo);
                }
            }
            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }


        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);
     
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
    }
}
