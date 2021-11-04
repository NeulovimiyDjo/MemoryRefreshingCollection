using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SharedProjects.Encryption;
using SharedProjects.Encryption.Helpers;
using Xunit;

namespace UnitTests.Encryption.Helpers
{
    [Collection("EncryptionKeys")]
    public class CertSignHelperTests : IDisposable
    {
        private readonly CertSignHelper _signManager;


        public CertSignHelperTests(EncryptionKeysFixture encryptionKeysFixture)
        {
            RSA keyPair = encryptionKeysFixture.RSAKeyPair;
            IEncryptorManager encryptorManager = new EncryptorManager(new HybridRSAAESEncryptor(keyPair, keyPair));
            _signManager = new CertSignHelper(encryptorManager);
        }

        public void Dispose()
        {
        }




        [Theory]
        [InlineData("")]
        [InlineData("TestMe")]
        [InlineData("LsdfjsldfjLL sdfj LLDLFSo )SDAF)&07 F)@))*$$ sdf_*_*(_(DSA_F*S&DF)S&DF) &&)DSA*F)SD&F)&F SDFFFFFFFFFFFFFFF x")]
        public void Sign_Then_Validate_Returns_True(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            byte[] signedData = _signManager.Sign(dataBytes);
            bool isValid = _signManager.Validate(signedData);

            Assert.True(isValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("TestMe")]
        [InlineData("LsdfjsldfjLL sdfj LLDLFSo )SDAF)&07 F)@))*$$ sdf_*_*(_(DSA_F*S&DF)S&DF) &&)DSA*F)SD&F)&F SDFFFFFFFFFFFFFFF x")]
        public void Sign_Then_GetData_Returns_Same_Value(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            byte[] signedData = _signManager.Sign(dataBytes);
            byte[] dataFromSignedData = _signManager.GetData(signedData);

            string dataFromSignedDataStr = Encoding.UTF8.GetString(dataFromSignedData);
            Assert.Equal(data, dataFromSignedDataStr);
        }

        [Fact]
        public void Sign_ThrowsOnNull()
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                _signManager.Sign(null);
            });
        }

        [Fact]
        public void Validate_ThrowsOnNull()
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                _signManager.Validate(null);
            });
        }


        [Theory]
        [InlineData("", "")]
        [InlineData("xx", "xx")]
        [InlineData("xx", "")]
        [InlineData("xx", "yy")]
        public void Sign_OutputIsDifferent(string data1, string data2)
        {
            byte[] dataBytes1 = Encoding.UTF8.GetBytes(data1);
            byte[] dataBytes2 = Encoding.UTF8.GetBytes(data2);

            byte[] signedData1 = _signManager.Sign(dataBytes1);
            byte[] signedData2 = _signManager.Sign(dataBytes2);

            Assert.False(signedData1.SequenceEqual(signedData2));
        }
    }
}
