using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using SharedProjects.Encryption;

namespace EncryptDecrypt
{
    class Program
    {
        public static void Main()
        {
            string key = "encryptionKey";
            var sha256 = SHA256.Create();
            byte[] hashOfKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));


            var encryptorManager = new EncryptorManager(new AESEncryptor(hashOfKey));
            var encryptedPassword = encryptorManager.EncryptStrToBase64("password");
            Console.WriteLine("Encrypted Password: " + encryptedPassword);

            var decrptedPassword = encryptorManager.DecryptBase64ToStr(encryptedPassword);
            Console.WriteLine("Decrypted Password: " + decrptedPassword);
        }
    }
}
