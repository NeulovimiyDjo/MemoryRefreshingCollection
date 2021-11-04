using System;
using System.Security.Cryptography;
using Xunit;

namespace UnitTests.Encryption
{
    public class EncryptionKeysFixture : IDisposable
    {
        public byte[] SymmetricKey { get; private set; } = new byte[256 / 8];
        public RSA RSAKeyPair { get; private set; } = new RSACng();

        public EncryptionKeysFixture()
        {
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(SymmetricKey); // Fill symmetricKey with random bytes.
        }

        public void Dispose()
        {
        }
    }


    [CollectionDefinition("EncryptionKeys")]
    public class EncryptionKeysCollection : ICollectionFixture<EncryptionKeysFixture>
    {
    }
}
