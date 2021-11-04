using ChunkedDataTransfer;
using System.Threading.Tasks;

namespace QRCopyPaste
{
    public class DesktopQRDataSender : IDataSender
    {
        public event QRImageChangedEventHandler OnQRImageChanged;


        public async Task SendAsync(string data)
        {
            var imageSource = QRSenderHelper.CreateQRWritableBitampFromString(data);
            this.OnQRImageChanged?.Invoke(imageSource);
            await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
        }

        public void Stop()
        {
            this.OnQRImageChanged?.Invoke(null);
        }
    }
}
