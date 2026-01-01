using SharedProjects.Encryption;
using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Encryption.Tests.Encryptors
{
    [Collection("EncryptionKeys")]
    public class AESEncryptorTests : IDisposable
    {
        private byte[] symmetricKey;


        public AESEncryptorTests(EncryptionKeysFixture encryptionKeysFixture)
        {
            this.symmetricKey = encryptionKeysFixture.SymmetricKey;
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
            var hybridRSAAESEncryptor = new AESEncryptor(this.symmetricKey);


            // Act
            byte[] encrypted = hybridRSAAESEncryptor.Encrypt(dataBytes);
            byte[] actualBytes = hybridRSAAESEncryptor.Decrypt(encrypted);


            // Assert
            string actual = Encoding.UTF8.GetString(actualBytes);
            Assert.Equal(data, actual);
        }
    }
}
