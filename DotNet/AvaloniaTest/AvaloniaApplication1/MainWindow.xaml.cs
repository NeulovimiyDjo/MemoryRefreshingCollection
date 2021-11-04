using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.IO;
using System.Windows.Forms;

namespace AvaloniaApplication1
{
    public class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private bool firstShow = true;
        private bool exitMode = false;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();
            var keyHooksLogicSetter = new KeyHooksLogicSetter(this);
            keyHooksLogicSetter.SetupKeyboardHooks();
            this.Closed += new System.EventHandler((s,e) => keyHooksLogicSetter.Dispose());

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var iconStream = assets.Open(new Uri($"avares://{typeof(Program).Assembly.GetName().Name}/Assets/AppIcon.ico"));
            this.Icon = new WindowIcon(iconStream);

            this.Closing += (s, e) =>
            {
                if (!exitMode)
                {
                    this.Hide(); e.Cancel = true;
                }
            };

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.DoubleClick += new EventHandler((s,e) => { this.Show(); } );
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Exit", null, (s, e) => { exitMode = true; this.Close(); });
            notifyIcon.ContextMenuStrip = menu;
            //var ms = new MemoryStream();
            //this.Icon.Save(ms);
            iconStream.Position = 0;
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Visible = true;


#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void Show()
        {
            //if (!firstShow)
            {
                base.Show();
            }

            firstShow = false;
        }
    }
}
