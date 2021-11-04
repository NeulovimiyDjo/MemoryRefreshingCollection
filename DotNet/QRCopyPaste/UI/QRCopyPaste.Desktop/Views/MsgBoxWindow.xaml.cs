using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QRCopyPaste
{
    public partial class MsgBoxWindow : Window
    {
        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No
        }


        public MsgBoxWindow()
        {
            InitializeComponent();
        }



        public static Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons)
        {
            var msgBoxWindow = new MsgBoxWindow()
            {
                Title = title
            };

            var textBox = (TextBox)msgBoxWindow.FindName("Text");
            textBox.Text = text;
            textBox.IsReadOnly = true;

            var buttonPanel = (StackPanel)msgBoxWindow.FindName("Buttons");

            var res = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool def = false)
            {
                var btn = new Button { Content = caption };
                btn.Width = 50;
                btn.FontSize = 22;
                btn.Click += (_, __) => {
                    res = r;
                    msgBoxWindow.Close();
                };
                buttonPanel.Children.Add(btn);
                if (def)
                    res = r;
            }

            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
                AddButton("Ok", MessageBoxResult.Ok, true);
            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, true);
            }

            if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
                AddButton("Cancel", MessageBoxResult.Cancel, true);


            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgBoxWindow.Closed += delegate { tcs.TrySetResult(res); };
            if (parent != null)
                msgBoxWindow.ShowDialog();
            else
                msgBoxWindow.Show();

            return tcs.Task;
        }
    }
}
