using System;
using System.Text;

namespace ChunkedDataTransfer
{
    internal static class ConvertHelper
    {
        internal static (string, string) GetZippedDataStrToSend<TData>(TData data)
        {
            string zippedDataStrToSend;
            string dataType;

            if (data is string unzippedDataStr)
            {
                var unzippedDataBytes = Encoding.UTF8.GetBytes(unzippedDataStr);
                zippedDataStrToSend = ZipHelper.GetZippedStringDataToSend(unzippedDataBytes);
                dataType = Constants.StringTypeName;
            }
            else if (data is byte[] unzippedDataBytes)
            {
                zippedDataStrToSend = ZipHelper.GetZippedStringDataToSend(unzippedDataBytes);
                dataType = Constants.ByteArrayTypeName;
            }
            else
            {
                throw new NotSupportedException($"Unsupported data type {data.GetType()} in {nameof(GetZippedDataStrToSend)}.");
            }

            return (zippedDataStrToSend, dataType);
        }


        internal static object GetInitialDataFromZippedDataStr(string zippedDataStr, string dataType)
        {
            if (dataType == Constants.StringTypeName)
            {
                var unzippedDataBytes = ZipHelper.GetUnzippedDataBytes(zippedDataStr);
                var unzippedDataStr = Encoding.UTF8.GetString(unzippedDataBytes);
                return unzippedDataStr;
            }
            else if (dataType == Constants.ByteArrayTypeName)
            {
                var unzippedDataBytes = ZipHelper.GetUnzippedDataBytes(zippedDataStr);
                return unzippedDataBytes;
            }
            else
            {
                throw new Exception($"Unsupported data type {dataType} in {nameof(GetInitialDataFromZippedDataStr)}.");
            }
        }
    }
}
