using System;
using System.Net;

namespace SharedProjects.Encryption.Helpers
{
    public static class ByteConvertHelper
    {
        public static byte[] IntToBytes(int value)
        {
            int bigEndianInt = IPAddress.HostToNetworkOrder(value);
            return BitConverter.GetBytes(bigEndianInt);
        }

        public static int BytesToInt(byte[] bytes)
        {
            int bigEndianInt = BitConverter.ToInt32(bytes, 0);
            return IPAddress.NetworkToHostOrder(bigEndianInt);
        }
    }
}
