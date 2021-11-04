using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace QRXamarinTest
{
    public class MainPage : ContentPage
    {
        private static int count = 0;
        private static Stopwatch sw = new Stopwatch();

        public MainPage()
        {
            sw.Start();

            var scanButton = new Button
            {
                Text = "Scan",
            };

            scanButton.Clicked += async (s,e) =>
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

                scanPage.OnScanResult += (result) =>
                {
                    count++;
                    if (count % 50 == 1)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Scanned result",
                                $"text = {result.Text}; count = {count}; sw.ElapsedMilliseconds = {sw.ElapsedMilliseconds}",
                                "OK"
                            );
                        });

                        Clipboard.SetTextAsync($"Copied:\n\n{result.Text}");
                    }
                };

                await Navigation.PushAsync(scanPage);
            };




            var displayButton = new Button
            {
                Text = "Display",
            };

            displayButton.Clicked += async (s, e) =>
            {
                var barcodePage = new BarcodePage();
                await Navigation.PushAsync(barcodePage);

                barcodePage.SetBarcodeValue("test1");
                await Task.Delay(1000);
                barcodePage.SetBarcodeValue("test2");
                await Task.Delay(1000);
                barcodePage.SetBarcodeViewVisibility(false);
            };




            var stackLayout = new StackLayout();
            stackLayout.Children.Add(scanButton);
            stackLayout.Children.Add(displayButton);
            this.Content = stackLayout;
        }
    }
}
