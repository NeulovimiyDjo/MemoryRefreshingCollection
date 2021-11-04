using ChunkedDataTransfer;
using System;
using System.Threading.Tasks;

namespace QRCopyPaste.Mobile
{
    public class MobileQRDataSender : IDataSender
    {
        private readonly Action<string> sendAction;
        private readonly Action stopAction;

        public MobileQRDataSender(
            Action<string> sendAction,
            Action stopAction)
        {
            this.sendAction = sendAction;
            this.stopAction = stopAction;
        }


        public async Task SendAsync(string data)
        {
            this.sendAction.Invoke(data);
            await Task.Delay(500);
        }

        public void Stop()
        {
            this.stopAction.Invoke();
        }
    }
}
