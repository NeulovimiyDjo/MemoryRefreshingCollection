using System.Security.Cryptography;

namespace SharedProjects.Encryption
{
    public class RSAEncryptor : IEncryptor
    {
        private RSA publicKey;
        private RSA privateKey;


        public RSAEncryptor(RSA publicKey, RSA privateKey)
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
            byte[] dataWithCheckSum = CheckSumHelper.GetDataWithCheckSum(data);
            return publicKey.Encrypt(dataWithCheckSum, RSAEncryptionPadding.OaepSHA256);
        }

        private static byte[] DecryptImpl(byte[] encryptedData, RSA privateKey)
        {
            var dataWithCheckSum = privateKey.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
            return CheckSumHelper.GetValidatedData(dataWithCheckSum);
        }
    }
}
