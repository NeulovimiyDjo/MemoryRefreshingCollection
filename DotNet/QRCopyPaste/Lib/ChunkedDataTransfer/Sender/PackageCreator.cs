using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ChunkedDataTransfer
{
    internal class PackageCreator
    {
        private readonly int _chunkSize;

        internal PackageCreator(int chunkSize)
        {
            this._chunkSize = chunkSize;
        }


        internal Package CreatePackage<TData>(TData data)
        {
            var (zippedDataStrToSend, dataType) = ConvertHelper.GetZippedDataStrToSend(data);
            var package = CreatePackageFromString(zippedDataStrToSend, dataType);
            return package;
        }


        private Package CreatePackageFromString(string data, string dataType)
        {
            var dataHash = HashHelper.GetStringHash(data);
            string objectID = dataHash;

            var dataParts = SplitHelper.SplitStringToChunks(data, this._chunkSize).ToArray();
            var dataPartsMessages = CreateDataPartsMessages(dataParts, objectID);

            var packageInfoMessage = CreatePackageInfoMessage(dataPartsMessages, dataType, dataHash, objectID);

            var package = new Package
            {
                PackageInfoMessage = packageInfoMessage,
                DataPartsMessages = dataPartsMessages,
            };

            return package;
        }


        private static string CreatePackageInfoMessage(
            string[] dataParts, string dataType, string dataHash, string objectID)
        {
            var packageInfoMessage = new PackageInfoMessage
            {
                MsgIntegrity = Constants.PackageInfoMessageIntegrityCheckID,
                NumberOfParts = dataParts.Length,
                DataType = dataType,
                DataHash = dataHash,
                ObjectID = objectID,
            };

            var packageInfoMessageStr = JsonSerializer.Serialize(packageInfoMessage);
            return packageInfoMessageStr;
        }


        private static string[] CreateDataPartsMessages(string[] dataParts, string objectID)
        {
            var dataPartsMessages = new List<string>();

            for (int i = 0; i < dataParts.Length; i++)
            {
                var dataPartMessage = new DataPartMessage
                {
                    MsgIntegrity = Constants.DataPartMessageIntegrityCheckID,
                    PartID = i,
                    Data = dataParts[i],
                    DataHash = HashHelper.GetStringHash(dataParts[i]),
                    ObjectID = objectID,
                };

                var dataPartMessageStr = JsonSerializer.Serialize(dataPartMessage);
                dataPartsMessages.Add(dataPartMessageStr);
            }

            return dataPartsMessages.ToArray();
        }
    }
}
