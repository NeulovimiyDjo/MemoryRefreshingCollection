using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChunkedDataTransfer
{
    public class ChunkedDataSender
    {
        public event ProgressChangedEventHandler OnProgressChanged;
        public int ChunkSize { get; set; } = 500;


        private Package _lastPackage = null;
        private bool _stopRequested = false;
        private bool _isRunning = false;
        private readonly IDataSender _dataSender;

        public ChunkedDataSender(IDataSender dataSender)
        {
            this._dataSender = dataSender;
        }


        public async Task SendAsync<TData>(TData data)
        {
            if (this._isRunning)
                throw new Exception("Data sending is already in progress.");
            this._isRunning = true;

            if (data is null)
                throw new ArgumentNullException($"{nameof(data)} is null in {nameof(SendAsync)}");

            if (data is string dataStr && dataStr == string.Empty
                || data is byte[] dataBytes && dataBytes.Length == 0)
                throw new Exception($"{nameof(data)} is empty in {nameof(SendAsync)}");

            var packageCreator = new PackageCreator(this.ChunkSize);
            var package = packageCreator.CreatePackage(data);
            this._lastPackage = package;

            await this._dataSender.SendAsync(package.PackageInfoMessage);
            await this.SendAllDataPartsAsync(package.DataPartsMessages);

            this._dataSender.Stop();
            this._isRunning = false;
            this._stopRequested = false;
        }


        public async Task ResendLastAsync(int[] selectiveIDs)
        {
            if (this._lastPackage == null)
                throw new Exception("There is nothing to resend.");

            if (this._isRunning)
                throw new Exception("Data sending is already in progress.");
            this._isRunning = true;

            var package = this._lastPackage;

            await this._dataSender.SendAsync(package.PackageInfoMessage);
            await this.SendAllDataPartsAsync(package.DataPartsMessages, selectiveIDs);

            this._dataSender.Stop();
            this._isRunning = false;
            this._stopRequested = false;
        }


        public void StopSending()
        {
            this._dataSender.Stop();
            this._stopRequested = true;
        }


        private async Task SendAllDataPartsAsync(string[] dataParts, int[] selectiveIDs = null)
        {
            this.OnProgressChanged?.Invoke(1);

            var numberOfParts = dataParts.Length;
            for (int i = 0; i < numberOfParts; i++)
            {
                if (this._stopRequested)
                    return;

                int progress = 1 + 99 * (i + 1) / numberOfParts;
                this.OnProgressChanged?.Invoke(progress);

                if (selectiveIDs != null && !selectiveIDs.Contains(i))
                    continue;

                string dataPart = dataParts[i];

                await this._dataSender.SendAsync(dataPart);
            }
        }
    }
}
