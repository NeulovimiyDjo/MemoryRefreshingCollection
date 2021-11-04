using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;

namespace QRCopyPaste
{
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("app.json", optional: false, reloadOnChange: true);

            this.Configuration = builder.Build();

            this.ConfigureQRSenderAndLoaderSettings();
        }


        private void ConfigureQRSenderAndLoaderSettings()
        {
            var appsettings = Configuration.GetSection("AppSettings");


            QRSenderSettings.ChunkSize = int.Parse(
                appsettings.GetSection(nameof(QRSenderSettings))
                .GetSection(nameof(QRSenderSettings.ChunkSize)).Value
            );

            QRSenderSettings.SendDelayMilliseconds = int.Parse(
                appsettings.GetSection(nameof(QRSenderSettings))
                .GetSection(nameof(QRSenderSettings.SendDelayMilliseconds)).Value
            );


            QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds = int.Parse(
                appsettings.GetSection(nameof(QRReceiverSettings))
                .GetSection(nameof(QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds)).Value
            );

            QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead = int.Parse(
                appsettings.GetSection(nameof(QRReceiverSettings))
                .GetSection(nameof(QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead)).Value
            );
        }
    }
}
