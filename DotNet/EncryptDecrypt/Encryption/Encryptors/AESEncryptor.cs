using System;
using System.IO;
using System.Security.Cryptography;

namespace SharedProjects.Encryption
{
    public class AESEncryptor : IEncryptor
    {
        private byte[] key;


        public AESEncryptor(byte[] key)
        {
            this.SetEncryptionKey(key);
        }

        public void SetEncryptionKey(byte[] key)
        {
            this.key = key;
        }




        public byte[] Encrypt(byte[] data)
        {
            return EncryptImpl(data, this.key);
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            return DecryptImpl(encryptedData, this.key);
        }




        private static byte[] EncryptImpl(byte[] data, byte[] key)
        {
            byte[] dataWithCheckSum = CheckSumHelper.GetDataWithCheckSum(data);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encrypter = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length); //Prepend IV to data

                    using (var cs = new CryptoStream(ms, encrypter, CryptoStreamMode.Write))
                    using (var bw = new BinaryWriter(cs))
                    {
                        bw.Write(dataWithCheckSum); //Encrypt data
                    }

                    return ms.ToArray();
                }
            }
        }

        private static byte[] DecryptImpl(byte[] encryptedDataWithIV, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                //Get first 16 bytes of IV and use it to decrypt
                var iv = new byte[16];
                Array.Copy(encryptedDataWithIV, 0, iv, 0, iv.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    // Get encrypted data part
                    var encryptedDataWithIVLength = encryptedDataWithIV.Length - iv.Length;
                    var encryptedData = new byte[encryptedDataWithIVLength];
                    Array.Copy(encryptedDataWithIV, iv.Length, encryptedData, 0, encryptedDataWithIVLength);

                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    using (var bw = new BinaryWriter(cs))
                    {
                        bw.Write(encryptedData); //Decrypt data
                    }

                    byte[] dataWithCheckSum = ms.ToArray();

                    return CheckSumHelper.GetValidatedData(dataWithCheckSum);
                }
            }
        }
    }
}
