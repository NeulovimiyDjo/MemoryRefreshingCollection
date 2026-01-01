using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using MsgBox;

namespace AvaloniaApplication1
{
    internal class KeyHooksLogicSetter : IDisposable
    {
        private GlobalKeyboardHook _globalKeyboardHook;
        private readonly Window mainWindow;

        public KeyHooksLogicSetter(Window mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            //Debug.WriteLine(e.KeyboardData.VirtualCode);

            if (e.KeyboardData.VirtualCode != GlobalKeyboardHook.VkSnapshot)
                return;

            // seems, not needed in the life.
            //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
            //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
            //{
            //    MessageBox.Show("Alt + Print Screen");
            //    e.Handled = true;
            //}
            //else

            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                MessageBox.Show(this.mainWindow, "Print Screen", "Test title", MessageBox.MessageBoxButtons.Ok);
                e.Handled = true;
            }
        }

        public void Dispose()
        {
            _globalKeyboardHook?.Dispose();
        }
    }
}
