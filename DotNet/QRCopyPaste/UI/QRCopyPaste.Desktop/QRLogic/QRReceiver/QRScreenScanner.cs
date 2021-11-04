using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ZXing;

namespace QRCopyPaste
{
    public class QRScreenScanner
    {
        public event QRTextDataReceivedEventHandler OnQRTextDataReceived;
        public event ErrorEventHandler OnError;


        private static bool _stopRequested = false;
        private static bool _isRunning = false;

        public bool StartScanning()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequestedAsync());
                return true;
            }

            return false;
        }


        private async Task RunScansUntilStopRequestedAsync()
        {
            while (!_stopRequested)
            {
                try
                {
                    var data = await WaitForSuccessfullyDecodedQRAsync();
                    this.OnQRTextDataReceived?.Invoke(data);
                }
                catch (Exception ex)
                {
                    this.OnError?.Invoke(ex.Message);
                }
            }
        }

        private readonly Stopwatch sw = new Stopwatch();
        private async Task<string> WaitForSuccessfullyDecodedQRAsync()
        {
            Result barcodeResult = null;
            while (barcodeResult == null)
            {
                var bitmap = QRMessageScannerHelper.CreateBitmapFromScreen();
                barcodeResult = QRMessageScannerHelper.GetBarcodeResultFromQRBitmap(bitmap);

                long minDelayMs = QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds;
                long elapsedMs = this.sw.ElapsedMilliseconds;
                int timeToWaitMs = (int)(minDelayMs - elapsedMs);
                if (timeToWaitMs > 0)
                    await Task.Delay(timeToWaitMs);
            }

            return barcodeResult.Text;
        }
    }
}
