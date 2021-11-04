using System;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing;

namespace QRXamForms.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamain-quickstart"));
            OnScanResultCommand = new Command<Result>(r => OnScanResult(r));
            sw.Start();
        }

        public ICommand OpenWebCommand { get; }
        public ICommand OnScanResultCommand { get; }

        private static int count = 0;
        private static Stopwatch sw = new Stopwatch();
        private void OnScanResult(Result result)
        {
            count++;
            if (count % 10 == 1)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Scanned result",
                        $"text = {result.Text}; count = {count}; sw.ElapsedMilliseconds = {sw.ElapsedMilliseconds}",
                        "OK"
                    );
                });
            }
        }
    }
}