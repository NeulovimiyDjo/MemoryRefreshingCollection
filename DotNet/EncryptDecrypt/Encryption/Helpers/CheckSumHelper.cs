using System;
using System.Linq;
using System.Security.Cryptography;

namespace SharedProjects.Encryption
{
    public static class CheckSumHelper
    {
        private const int CheckSumLength = 256 / 8;


        public static byte[] GetDataWithCheckSum(byte[] data)
        {
            var hasher = new SHA256Managed();
            byte[] checkSum = hasher.ComputeHash(data);
            return checkSum.Concat(data).ToArray();
        }

        public static byte[] GetValidatedData(byte[] dataWithCheckSum)
        {
            byte[] data = dataWithCheckSum.Skip(CheckSumLength).ToArray();


            // Validate that prepended checksum is equal to data hash.
            var hasher = new SHA256Managed();
            byte[] calculatedCheckSum = hasher.ComputeHash(data);
            byte[] prependedCheckSum = dataWithCheckSum.Take(CheckSumLength).ToArray();
            if (dataWithCheckSum.Length < CheckSumLength
                || !prependedCheckSum.SequenceEqual(calculatedCheckSum))
            {
                throw new Exception("CheckSum check failed.");
            }


            return data;
        }
    }
}
