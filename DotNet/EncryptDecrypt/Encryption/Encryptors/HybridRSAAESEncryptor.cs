using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace SharedProjects.Encryption
{
    /// <summary>
    /// Generates random symmetric AES key and encrypts data with AES,
    /// then encrypts AES key with RSA and prepends it to encrypted data.
    /// </summary>
    public class HybridRSAAESEncryptor : IEncryptor
    {
        private const int AESKeyLength = 256 / 8;
        private const int IntSize = 4;


        private RSA publicKey;
        private RSA privateKey;


        public HybridRSAAESEncryptor(RSA publicKey, RSA privateKey)
        {
            this.SetEncryptionKeys(publicKey, privateKey);
        }

        public void SetEncryptionKeys(RSA publicKey, RSA privateKey)
        {
            this.publicKey = publicKey;
            this.privateKey = privateKey;
        }




        public byte[] Encrypt(byte[] data)
        {
            return EncryptImpl(data, this.publicKey);
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            return DecryptImpl(encryptedData, this.privateKey);
        }





        private static byte[] EncryptImpl(byte[] data, RSA publicKey)
        {
            byte[] symmetricKey = GenerateRandomAESKey();

            var aesEncryptor = new AESEncryptor(symmetricKey);
            byte[] encryptedData = aesEncryptor.Encrypt(data);

            var rsaEncryptor = new RSAEncryptor(publicKey, null);
            byte[] encryptedSymmetricKey = rsaEncryptor.Encrypt(symmetricKey);

            byte[] encryptedSymmetricKeyLengthBytes = IntToBytes(encryptedSymmetricKey.Length);

            byte[] encryptedSymmetricKey_EncryptedData = encryptedSymmetricKey.Concat(encryptedData).ToArray();
            return encryptedSymmetricKeyLengthBytes.Concat(encryptedSymmetricKey_EncryptedData).ToArray();
        }


        private static byte[] DecryptImpl(byte[] encryptedSymmetricKeyLengthBytes_encryptedSymmetricKey_EncryptedData, RSA privateKey)
        {
            byte[] encryptedSymmetricKeyLengthBytes = encryptedSymmetricKeyLengthBytes_encryptedSymmetricKey_EncryptedData.Take(IntSize).ToArray();
            int encryptedSymmetricKeyLength = BytesToInt(encryptedSymmetricKeyLengthBytes);

            byte[] encryptedSymmetricKey = encryptedSymmetricKeyLengthBytes_encryptedSymmetricKey_EncryptedData
                .Skip(IntSize).Take(encryptedSymmetricKeyLength).ToArray();

            var rsaEncryptor = new RSAEncryptor(null, privateKey);
            byte[] symmetricKey = rsaEncryptor.Decrypt(encryptedSymmetricKey);


            byte[] encryptedData = encryptedSymmetricKeyLengthBytes_encryptedSymmetricKey_EncryptedData
                .Skip(IntSize + encryptedSymmetricKeyLength).ToArray();

            var aesEncryptor = new AESEncryptor(symmetricKey);
            return aesEncryptor.Decrypt(encryptedData);
        }



        private static byte[] IntToBytes(int value)
        {
            int bigEndianInt = IPAddress.HostToNetworkOrder(value);
            return BitConverter.GetBytes(bigEndianInt);
        }

        private static int BytesToInt(byte[] bytes)
        {
            int bigEndianInt = BitConverter.ToInt32(bytes, 0);
            return IPAddress.NetworkToHostOrder(bigEndianInt);
        }



        private static byte[] GenerateRandomAESKey()
        {
            byte[] symmetricKey = new byte[AESKeyLength];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(symmetricKey); // Fill symmetricKey with random bytes.
            return symmetricKey;
        }
    }
}
