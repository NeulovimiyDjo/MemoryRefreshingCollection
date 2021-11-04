using System;
using System.Collections.Generic;
using System.Linq;

namespace ChunkedDataTransfer
{
    public class ChunkedDataReceiver
    {
        public event StringDataReceivedEventHandler OnStringDataReceived;
        public event ByteDataReceivedEventHandler OnByteDataReceived;
        public event NotificationEventHandler OnNotification;
        public event ReceivingStartedEventHandler OnReceivingStarted;
        public event ReceivingStoppedEventHandler OnReceivingStopped;
        public event ChunkReceivedEventHandler OnChunkReceived;
        public event ProgressChangedEventHandler OnProgressChanged;


        private readonly Dictionary<string, Dictionary<int, string>> _receivedItemsCache;
        private readonly Dictionary<string, PackageInfoMessage> _receivedPackageInfoMessages;
        private bool _isRunning = false;
        private string _currentlyReceivingObjectID;

        public ChunkedDataReceiver()
        {
            this._receivedItemsCache = new Dictionary<string, Dictionary<int, string>>();
            this._receivedPackageInfoMessages = new Dictionary<string, PackageInfoMessage>();
        }


        public void StartReceiving()
        {
            this._isRunning = true;
        }


        public void ClearCache()
        {
            this._receivedItemsCache.Clear();
            this._receivedPackageInfoMessages.Clear();
            this.OnProgressChanged?.Invoke(0);
            this.OnReceivingStopped?.Invoke(null);
        }


        public void StopReceivingAll()
        {
            this.OnReceivingStopped?.Invoke(null);
            this._isRunning = false;
            this._currentlyReceivingObjectID = null;
        }


        public void StopReceiving(string objectID)
        {
            var packageInfoMessage = this._receivedPackageInfoMessages[objectID];
            var dataParts = this._receivedItemsCache[packageInfoMessage.DataHash];

            this.NotifyIfNotAllDataPartsReceived(packageInfoMessage, dataParts);

            this.OnReceivingStopped?.Invoke(objectID);
            this._currentlyReceivingObjectID = null;
        }


        public void ProcessChunk(string dataChunk)
        {
            if (!this._isRunning)
                return;

            if (JsonHelper.TryDeserialize<PackageInfoMessage>(dataChunk, out var packageInfoMessage)
                && packageInfoMessage.MsgIntegrity == Constants.PackageInfoMessageIntegrityCheckID)
            {
                this.ProcessPackageInfoMessage(packageInfoMessage);
                return;
            }

            if (this._currentlyReceivingObjectID is null)
                return;

            if (JsonHelper.TryDeserialize<DataPartMessage>(dataChunk, out var dataPartMessage)
                && dataPartMessage.MsgIntegrity == Constants.DataPartMessageIntegrityCheckID)
            {
                this.ProcessDataPartMessage(dataPartMessage);
            }
        }


        private void ProcessPackageInfoMessage(PackageInfoMessage packageInfoMessage)
        {
            if (!this._receivedPackageInfoMessages.ContainsKey(packageInfoMessage.DataHash))
                this._receivedPackageInfoMessages.Add(packageInfoMessage.ObjectID, packageInfoMessage);

            this.OnProgressChanged?.Invoke(1);
            this.OnReceivingStarted?.Invoke(packageInfoMessage.ObjectID);
            this._currentlyReceivingObjectID = packageInfoMessage.ObjectID;
        }


        private void ProcessDataPartMessage(DataPartMessage dataPartMessage)
        {
            if (!this._receivedPackageInfoMessages.ContainsKey(dataPartMessage.ObjectID))
                return;

            this.OnChunkReceived?.Invoke(dataPartMessage.ObjectID);

            var packageInfoMessage = this._receivedPackageInfoMessages[dataPartMessage.ObjectID];
            var dataParts = this.GetOrAddCurrentlyReceivingDataParts(packageInfoMessage);

            if (!dataParts.ContainsKey(dataPartMessage.PartID)
                && HashHelper.GetStringHash(dataPartMessage.Data) == dataPartMessage.DataHash)
            {
                dataParts.Add(dataPartMessage.PartID, dataPartMessage.Data);
            }

            int progress = 1 + 99 * dataParts.Count / packageInfoMessage.NumberOfParts;
            this.OnProgressChanged?.Invoke(progress);

            if (dataParts.Count == packageInfoMessage.NumberOfParts)
                this.HandleDataReceived(packageInfoMessage, dataParts);
        }


        private Dictionary<int, string> GetOrAddCurrentlyReceivingDataParts(PackageInfoMessage packageInfoMessage)
        {
            if (!this._receivedItemsCache.ContainsKey(packageInfoMessage.DataHash))
                this._receivedItemsCache.Add(packageInfoMessage.DataHash, new Dictionary<int, string>());
            var dataParts = this._receivedItemsCache[packageInfoMessage.DataHash];
            return dataParts;
        }


        private void HandleDataReceived(PackageInfoMessage packageInfoMessage, Dictionary<int, string> dataParts)
        {
            var fullDataStr = string.Join("", dataParts.ToList().OrderBy(p => p.Key).Select(p => p.Value));
            this.ThrowIfFullDataHashIsWrong(packageInfoMessage, fullDataStr);

            var fullData = ConvertHelper.GetInitialDataFromZippedDataStr(fullDataStr, packageInfoMessage.DataType);
            this.NotifyDataReceived(fullData, packageInfoMessage.DataType);

            this.OnReceivingStopped?.Invoke(null);
            this._currentlyReceivingObjectID = null;
        }


        private void NotifyIfNotAllDataPartsReceived(PackageInfoMessage packageInfoMessage, Dictionary<int, string> dataParts)
        {
            if (dataParts.Count != packageInfoMessage.NumberOfParts)
            {
                var idsNotReceived = new List<string>();
                for (int i = 0; i < packageInfoMessage.NumberOfParts; i++)
                {
                    if (!dataParts.ContainsKey(i))
                        idsNotReceived.Add(i.ToString());
                }

                var missingPartsCount = packageInfoMessage.NumberOfParts - dataParts.Count;
                this.OnNotification?.Invoke(
                    $"Data was not fully received.\n" +
                    $"{dataParts.Count} out of {packageInfoMessage.NumberOfParts} parts received.\n" +
                    $"{missingPartsCount} parts missing.\n" +
                    $"Missing IDs list:\n" +
                    string.Join(" ", idsNotReceived)
                );
            }
        }


        private void ThrowIfFullDataHashIsWrong(PackageInfoMessage packageInfoMessage, string fullDataStr)
        {
            var dataHash = HashHelper.GetStringHash(fullDataStr);
            if (dataHash != packageInfoMessage.DataHash)
                throw new Exception($"Received data is incorrect. Hashes differ.");
        }


        private void NotifyDataReceived(object fullData, string dataType)
        {
            if (dataType == Constants.StringTypeName)
                this.OnStringDataReceived?.Invoke((string)fullData);
            else if (dataType == Constants.ByteArrayTypeName)
                this.OnByteDataReceived?.Invoke((byte[])fullData);
            else
                throw new Exception($"Unsupported data type {dataType} in {nameof(NotifyDataReceived)}.");
        }
    }
}
