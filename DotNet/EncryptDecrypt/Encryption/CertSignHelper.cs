using System.Linq;
using System.Security.Cryptography;
using static SharedProjects.Encryption.Helpers.ByteConvertHelper;

namespace SharedProjects.Encryption.Helpers
{
    public class CertSignHelper
    {
        private const int HashLength = 256 / 8;
        private const int IntSize = 4;
        private readonly IEncryptorManager _encryptorManager;

        public CertSignHelper(IEncryptorManager encryptorManager)
        {
            _encryptorManager = encryptorManager;
        }


        public byte[] Sign(byte[] data)
        {
            var hasher = new SHA256Managed();
            byte[] hash = hasher.ComputeHash(data);

            byte[] encryptedHash = _encryptorManager.Encrypt(hash);
            byte[] encryptedHashLengthBytes = IntToBytes(encryptedHash.Length);
            byte[] signedData = encryptedHashLengthBytes.Concat(encryptedHash.Concat(data)).ToArray();

            return signedData;
        }

        public byte[] GetData(byte[] signedData)
        {
            int encryptedPrependedHashLength = GetEncryptedPrependedHashLength(signedData);
            byte[] data = GetData(signedData, encryptedPrependedHashLength);
            return data;
        }

        public bool Validate(byte[] signedData)
        {
            int encryptedPrependedHashLength = GetEncryptedPrependedHashLength(signedData);
            byte[] prependedHash = GetPrependedHash(signedData, encryptedPrependedHashLength);
            byte[] data = GetData(signedData, encryptedPrependedHashLength);

            var hasher = new SHA256Managed();
            byte[] calculatedHash = hasher.ComputeHash(data);

            if (prependedHash.Length == HashLength && prependedHash.SequenceEqual(calculatedHash))
                return true;

            return false;
        }


        private static int GetEncryptedPrependedHashLength(byte[] signedData)
        {
            byte[] encryptedHashLengthBytes = signedData.Take(IntSize).ToArray();
            int encryptedHashLength = BytesToInt(encryptedHashLengthBytes);
            return encryptedHashLength;
        }

        private byte[] GetPrependedHash(byte[] signedData, int encryptedPrependedHashLength)
        {
            byte[] encryptedPrependedHash = signedData.Skip(IntSize).Take(encryptedPrependedHashLength).ToArray();
            byte[] prependedHash = _encryptorManager.Decrypt(encryptedPrependedHash);
            return prependedHash;
        }

        private static byte[] GetData(byte[] signedData, int encryptedPrependedHashLength)
        {
            return signedData.Skip(IntSize + encryptedPrependedHashLength).ToArray();
        }
    }
}
