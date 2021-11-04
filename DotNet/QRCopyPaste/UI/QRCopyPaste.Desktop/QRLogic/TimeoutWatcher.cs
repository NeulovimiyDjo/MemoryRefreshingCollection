using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QRCopyPaste.Desktop.QRLogic
{
    public class TimeoutWatcher
    {
        private bool isRunning;
        private readonly Stopwatch sw = new Stopwatch();
        private readonly Action callback;

        public TimeoutWatcher(Action callback)
        {
            this.callback = callback;
        }


        public void Start()
        {
            if (this.isRunning)
                return;

            this.Restart();

            Task.Run(async () =>
            {
                while (this.isRunning)
                {
                    int timeoutMs = QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead;
                    if (this.sw.ElapsedMilliseconds > timeoutMs)
                    {
                        this.callback();
                        this.isRunning = false;
                        return;
                    }

                    await Task.Delay(100);
                }
            });
        }


        public void Restart()
        {
            this.isRunning = true;
            this.sw.Restart();
        }


        public void Stop()
        {
            this.isRunning = false;
        }
    }
}
