using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.Wpf;
using System.Threading.Tasks;
using System.Windows;

namespace CefSharpTarget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitBrowser();
        }

        private void InitBrowser()
        {
            var currDir = System.IO.Directory.GetCurrentDirectory();
            var chromeDirPath = System.IO.Path.Combine(currDir, "chrome_dir");
            System.IO.Directory.CreateDirectory(chromeDirPath);

            var settings = new CefSettings();

            settings.RemoteDebuggingPort = 8881;

            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "localfolder",
                DomainName = "cefsharp",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(
                    rootFolder: chromeDirPath,
                    hostName: "cefsharp",
                    defaultPage: "index.html" // will default to index.html
                )
            });

            Cef.Initialize(settings);

            var browser = new ChromiumWebBrowser("localfolder://cefsharp/");

            this.mainGrid.Children.Add(browser);

            Task.Run(() => {
                Task.Delay(10000).GetAwaiter().GetResult();
                //browser.Reload(true);
            });
        }
    }
}
