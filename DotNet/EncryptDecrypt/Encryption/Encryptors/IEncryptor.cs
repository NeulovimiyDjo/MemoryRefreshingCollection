namespace SharedProjects.Encryption
{
    public interface IEncryptor
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] encryptedData);
    }
}
