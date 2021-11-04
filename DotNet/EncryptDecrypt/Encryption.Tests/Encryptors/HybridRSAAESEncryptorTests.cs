using SharedProjects.Encryption;
using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Encryption.Tests.Encryptors
{
    [Collection("EncryptionKeys")]
    public class HybridRSAAESEncryptorTests : IDisposable
    {
        private RSA rsaKeyPair;


        public HybridRSAAESEncryptorTests(EncryptionKeysFixture encryptionKeysFixture)
        {
            this.rsaKeyPair = encryptionKeysFixture.RSAKeyPair;
        }

        public void Dispose()
        {
        }




        [Theory]
        [InlineData("")]
        [InlineData("TestMe")]
        public void EncryptDecrypt_ReturnsSameValue(string data)
        {
            // Arrange
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            var hybridRSAAESEncryptor = new HybridRSAAESEncryptor(this.rsaKeyPair, this.rsaKeyPair);


            // Act
            byte[] encrypted = hybridRSAAESEncryptor.Encrypt(dataBytes);
            byte[] actualBytes = hybridRSAAESEncryptor.Decrypt(encrypted);


            // Assert
            string actual = Encoding.UTF8.GetString(actualBytes);
            Assert.Equal(data, actual);
        }
    }
}
