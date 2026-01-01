using System;
using System.Text;

namespace SharedProjects.Encryption
{
    public class EncryptorManager : IEncryptorManager
    {
        private readonly IEncryptor encryptor;


        public EncryptorManager(IEncryptor encryptor)
        {
            this.encryptor = encryptor;
        }


        public byte[] Encrypt(byte[] data)
        {
            return this.encryptor.Encrypt(data);
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            return this.encryptor.Decrypt(encryptedData);
        }



        public byte[] EncryptStrToByte(string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            return Encrypt(byteData);
        }

        public string DecryptByteToStr(byte[] encryptedData)
        {
            byte[] byteData = Decrypt(encryptedData);
            return Encoding.UTF8.GetString(byteData);
        }



        public string EncryptStrToBase64(string data)
        {
            byte[] encryptedByteData = EncryptStrToByte(data);
            return Convert.ToBase64String(encryptedByteData);
        }

        public string DecryptBase64ToStr(string encryptedData)
        {
            byte[] encryptedByteData = Convert.FromBase64String(encryptedData);
            return DecryptByteToStr(encryptedByteData);
        }
    }
}
