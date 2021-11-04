namespace SharedProjects.Encryption
{
    interface IEncryptorManager
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] encryptedData);

        byte[] EncryptStrToByte(string data);
        string DecryptByteToStr(byte[] encryptedData);

        string EncryptStrToBase64(string data);
        string DecryptBase64ToStr(string encryptedData);
    }
}
