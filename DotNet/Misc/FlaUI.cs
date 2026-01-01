using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using NUnit.Framework;
using System.Threading;

namespace FlaUIIntegrationTests
{
    [TestFixture]
    public class Test2
    {
        [Test]
        public void Test2Method()
        {
            using (var automation = new UIA3Automation())
            {
                var app = Application.Attach("Client.exe");
                //var app = Application.Launch(@"C:\Applications\Client\Client.exe", "/a:http://localhost:5000");

                app.WaitWhileMainHandleIsMissing();
                var mainWindow = app.GetMainWindow(automation);
                //SetWindowForeground(mainWindow);

                //RegisterEventsTest(automation, mainWindow);

                Assert.That(mainWindow, Is.Not.Null);

                AutomationElement someControl = GetAutocompleteControl(automation, mainWindow);
                //FillAutocompleteBySelectButton(someControl);
                FillAutocompleteByTyping(someControl);

                //FillTextboxDirectlyTest(someControl);

                //app.Close();
            }
        }

        private static void FillTextboxDirectlyTest(AutomationElement someControl)
        {
            var someTextBox = someControl.FindFirstByXPath("/Custom/Edit").AsTextBox();
            someTextBox.Text = "text";
        }

        private static void RegisterEventsTest(UIA3Automation automation, Window mainWindow)
        {
            var shrinkButton = mainWindow.FindFirstDescendant("53252");
            var enlargeButton = mainWindow.FindFirstDescendant("53253");

            shrinkButton.RegisterAutomationEvent(automation.EventLibrary.Invoke.InvokedEvent, TreeScope.Element, (element, evnt) =>
            {
                Assert.Warn("Shrink Clicked");
            });

            enlargeButton.RegisterAutomationEvent(automation.EventLibrary.Invoke.InvokedEvent, TreeScope.Element, (element, evnt) =>
            {
                Assert.Warn("Shrink Clicked");
            });
        }

        private static void SetWindowForeground(Window mainWindow)
        {
            mainWindow.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Maximized);
            mainWindow.SetForeground();
        }

        private static AutomationElement GetAutocompleteControl(UIA3Automation automation, Window mainWindow)
        {
            var someTooltip = mainWindow.FindFirstDescendant(x => x.ByName("Подотчётное лицо  *"));
            var treeWalker = automation.TreeWalkerFactory.GetControlViewWalker();
            var someControl = treeWalker.GetNextSibling(someTooltip);
            return someControl;
        }

        private static void FillAutocompleteBySelectButton(AutomationElement someControl)
        {
            var someSelectBtn = someControl.FindFirstDescendant(x => x.ByName("…"));
            someSelectBtn.Click();
        }

        private static void FillAutocompleteByTyping(AutomationElement someControl)
        {
            someControl.Click();
            Keyboard.Type("test");
            Thread.Sleep(500);
            Keyboard.Press(VirtualKeyShort.ENTER);
            Keyboard.Release(VirtualKeyShort.ENTER);
        }
    }
}