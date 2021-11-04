using ChunkedDataTransfer;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace QRCopyPaste.Mobile
{
    public class MainPage : ContentPage
    {
        public MainPage()
        {
            var startScanningButton = new Button
            {
                Text = "StartScanning",
            };

            startScanningButton.Clicked += async (s,e) =>
            {
                var scanPage = new ZXingScannerPage(
                    new MobileBarcodeScanningOptions
                    {
                        DelayBetweenContinuousScans = 10,
                        DelayBetweenAnalyzingFrames = 10,
                        UseFrontCameraIfAvailable = true,
                        UseNativeScanning = true,
                        TryHarder = true,
                    }
                );

                var chunkedDataReceiver = new ChunkedDataReceiver();
                chunkedDataReceiver.OnStringDataReceived += data =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Received data",
                            $"Copied to clipboard:\n\n{data}",
                            "OK"
                        );
                    });

                    Clipboard.SetTextAsync($"Copied: {data}");
                };

                scanPage.OnScanResult += (result) =>
                {
                    chunkedDataReceiver.ProcessChunk(result.Text);
                };

                chunkedDataReceiver.StartReceiving();


                await Navigation.PushAsync(scanPage);
            };




            var sendClipboardButton = new Button
            {
                Text = "SendClipboard",
            };

            sendClipboardButton.Clicked += async (s, e) =>
            {
                var barcodePage = new BarcodePage();
                await Navigation.PushAsync(barcodePage);

                var dataSender = new MobileQRDataSender(
                    data => barcodePage.SetBarcodeValue(data),
                    () => barcodePage.SetBarcodeViewVisibility(false)
                );
                var chunkedDataSender = new ChunkedDataSender(dataSender);

                string clipboardText = await Clipboard.GetTextAsync();
                await chunkedDataSender.SendAsync("change to clip");
            };




            var stackLayout = new StackLayout();
            stackLayout.Children.Add(startScanningButton);
            stackLayout.Children.Add(sendClipboardButton);
            this.Content = stackLayout;
        }
    }
}
