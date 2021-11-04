using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;

namespace ChunkedDataTransfer
{
    internal static class ZipHelper
    {
        internal static string GetZippedStringDataToSend(byte[] unzippedDataBytes)
        {
            var unzippedDataBytesStream = new MemoryStream(unzippedDataBytes);
            var zippedDataBytesStream = new MemoryStream();
            GZip.Compress(unzippedDataBytesStream, zippedDataBytesStream, true, 4096, 9);

            var zippedDataBytes = zippedDataBytesStream.ToArray();
            var zippedStringDataToSend = Convert.ToBase64String(zippedDataBytes);
            return zippedStringDataToSend;
        }


        internal static byte[] GetUnzippedDataBytes(string zippedDataStr)
        {
            var zippedDataBytesStream = new MemoryStream(Convert.FromBase64String(zippedDataStr));
            var unzippedDataBytesStream = new MemoryStream();
            GZip.Decompress(zippedDataBytesStream, unzippedDataBytesStream, true);

            var unzippedDataBytes = unzippedDataBytesStream.ToArray();
            return unzippedDataBytes;
        }
    }
}
