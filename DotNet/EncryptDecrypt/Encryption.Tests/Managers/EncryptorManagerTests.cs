using Moq;
using SharedProjects.Encryption;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace Encryption.Tests.Encryptors
{
    public class EncryptorManagerTests : IDisposable
    {
        private Mock<IEncryptor> encryptorMock;

        private byte[] testData1 = new byte[] { 1, 2, 3 };
        private byte[] testData2 = new byte[] { 4, 4, 4 };
        private string testDataStr1;
        private string testDataStr2;

        private byte[] testEncryptedData1 = new byte[] { 3, 2, 1 };
        private byte[] testEncryptedData2 = new byte[] { 5, 5, 5 };


        public EncryptorManagerTests()
        {
            this.testDataStr1 = Encoding.UTF8.GetString(this.testData1);
            this.testDataStr2 = Encoding.UTF8.GetString(this.testData2);


            this.encryptorMock = new Mock<IEncryptor>();

            this.encryptorMock.Setup(encryptor => encryptor.Encrypt(testData1)).Returns(testEncryptedData1);
            this.encryptorMock.Setup(encryptor => encryptor.Encrypt(testData2)).Returns(testEncryptedData2);
            this.encryptorMock.Setup(encryptor => encryptor.Encrypt(null)).Throws<Exception>();

            this.encryptorMock.Setup(encryptor => encryptor.Decrypt(testEncryptedData1)).Returns(testData1);
            this.encryptorMock.Setup(encryptor => encryptor.Decrypt(testEncryptedData2)).Returns(testData2);
            this.encryptorMock.Setup(encryptor => encryptor.Decrypt(null)).Throws<Exception>();
        }

        public void Dispose()
        {
        }





        [Fact]
        public void EncryptDecrypt_ByteToByte_ReturnsSameValue()
        {
            // Arrange
            var hybridRSAAESEncryptor = new EncryptorManager(this.encryptorMock.Object);


            // Act
            byte[] encrypted = hybridRSAAESEncryptor.Encrypt(this.testData1);
            byte[] actual = hybridRSAAESEncryptor.Decrypt(encrypted);


            // Assert
            Assert.Equal(this.testData1, actual);
        }


        [Fact]
        public void Encrypt_ByteToByte_ThrowsOnNull()
        {
            // Arrange
            var hybridRSAAESEncryptor = new EncryptorManager(this.encryptorMock.Object);


            Assert.ThrowsAny<Exception>(() =>
            {
                // Act
                byte[] encrypted = hybridRSAAESEncryptor.Encrypt(null);
            });
        }


        [Fact]
        public void Decrypt_ByteToByte_ThrowsOnNull()
        {
            // Arrange
            var hybridRSAAESEncryptor = new EncryptorManager(this.encryptorMock.Object);


            Assert.ThrowsAny<Exception>(() =>
            {
                // Act
                byte[] data = hybridRSAAESEncryptor.Decrypt(null);
            });
        }


        [Fact]
        public void Encrypt_ByteToByte_OutputIsDifferent()
        {
            // Arrange
            var hybridRSAAESEncryptor = new EncryptorManager(this.encryptorMock.Object);


            // Act
            byte[] encrypted1 = hybridRSAAESEncryptor.Encrypt(this.testData1);
            byte[] encrypted2 = hybridRSAAESEncryptor.Encrypt(this.testData2);


            // Assert
            Assert.False(encrypted1.SequenceEqual(encrypted2));
        }





        [Fact]
        public void EncryptDecrypt_StrToByte_ReturnsSameValue()
        {
            // Arrange
            var hybridRSAAESEncryptor = new EncryptorManager(this.encryptorMock.Object);


            // Act
            byte[] encrypted = hybridRSAAESEncryptor.EncryptStrToByte(this.testDataStr1);
            string actual = hybridRSAAESEncryptor.DecryptByteToStr(encrypted);


            // Assert
            Assert.Equal(this.testDataStr1, actual);
        }





        [Fact]
        public void EncryptDecrypt_StrToStr_ReturnsSameValue()
        {
            // Arrange
            var hybridRSAAESEncryptor = new EncryptorManager(this.encryptorMock.Object);


            // Act
            string encrypted = hybridRSAAESEncryptor.EncryptStrToBase64(this.testDataStr1);
            string actual = hybridRSAAESEncryptor.DecryptBase64ToStr(encrypted);


            // Assert
            Assert.Equal(this.testDataStr1, actual);
        }
    }
}
