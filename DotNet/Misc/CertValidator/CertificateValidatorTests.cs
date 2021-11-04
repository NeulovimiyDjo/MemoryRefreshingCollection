using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace UnitTests.Certs
{
    public class CertificateValidatorTests
    {
        private readonly CertificateValidator validator;
        private readonly CertificateValidator validatorWithNoTrustedSubrootInTrustStore;

        private readonly X509Certificate2 rootCert;
        private readonly X509Certificate2 subrootCert;
        private readonly X509Certificate2 untrustedRootCert;

        private readonly X509Certificate2 validCert;
        private readonly X509Certificate2 untrustedCert;
        private readonly X509Certificate2 expiredCert;

        public CertificateValidatorTests()
        {
            validator = new("test_data/trusted_root_and_subroot_certs_bundle.crt", "none");
            validatorWithNoTrustedSubrootInTrustStore = new("test_data/trusted_root_certs_bundle.crt", "none");

            rootCert = new X509Certificate2("test_data/root_cert.crt");
            subrootCert = new X509Certificate2("test_data/subroot_cert.crt");
            untrustedRootCert = new X509Certificate2("test_data/untrusted_root_cert.crt");

            validCert = new X509Certificate2("test_data/leaf_cert.crt");
            untrustedCert = new X509Certificate2("test_data/untrusted_leaf_cert.crt");
            expiredCert = new X509Certificate2("test_data/expired_leaf_cert.crt");
        }

        [Fact]
        public void Validate_ValidCert_ReturnsTrueWithoutErrors()
        {
            bool result = validator.Validate(validCert, null, out string error);
            Assert.True(result, "Valid cert should validate");
            Assert.Equal(error, string.Empty);
        }

        [Fact]
        public void Validate_ValidCert_WithProvidedChain_WithNoTrustedSubrootInTrustStore_ReturnsTrueWithoutErrors()
        {
            X509Chain ch = CreateTestChainForValidCert();
            bool result = validatorWithNoTrustedSubrootInTrustStore.Validate(validCert, ch, out string error);
            Assert.True(result, "Valid cert should validate with provided chain and without subroot added to trusted certs");
            Assert.Equal(error, string.Empty);
        }

        [Fact]
        public void Validate_ValidCert_WithoutProvidedChain_WithNoTrustedSubrootInTrustStore_ReturnsFalseWithAppropriateError()
        {
            bool result = validatorWithNoTrustedSubrootInTrustStore.Validate(validCert, null, out string error);
            Assert.False(result, "Valid cert should not validate without provided chain and without subroot added to trusted certs");
            Assert.Matches(@"(?:.|\n)*(?:could not be built to a trusted root authority|unable to get local issuer certificate)", error);
        }

        [Fact]
        public void Validate_UntrustedCert_ReturnsFalseWithAppropriateError()
        {
            bool result = validator.Validate(untrustedCert, null, out string error);
            Assert.False(result, "Untrusted cert should not validate");
            Assert.Matches(@"(?:.|\n)*(?:could not be built to a trusted root authority|unable to get local issuer certificate)", error);
        }

        [Fact]
        public void Validate_UntrustedCert_WithProvidedChain_WithNoTrustedSubrootInTrustStore_ReturnsFalseWithAppropriateError()
        {
            X509Chain ch = CreateTestChainForUntrustedCert();
            bool result = validatorWithNoTrustedSubrootInTrustStore.Validate(untrustedCert, ch, out string error);
            Assert.False(result, "Untrusted cert should not validate with provided chain and without subroot added to trusted certs");
            Assert.Matches(@"(?:.|\n)*(?:but terminated in a root certificate which is not trusted by the trust provider|self signed certificate in certificate chain)", error);
        }

        [Fact]
        public void Validate_ExpiredCert_ReturnsFalseWithAppropriateError()
        {
            bool result = validator.Validate(expiredCert, null, out string error);
            Assert.False(result, "Expired cert should not validate");
            Assert.Matches(@"(?:.|\n)*Subject: CN=leaf_cert(?:.|\n)*(?:is not within its validity period|certificate has expired)", error);
        }

        [Fact]
        public void HostnameIsAppropriateForCertificate_GoodHost_ReturnsTrueWithoutErrors()
        {
            bool result = CertificateValidator.HostnameIsAppropriateForCertificate("someOTHERhost", validCert, out string error);
            Assert.True(result, "Good simple host should return true");
            Assert.Equal(error, string.Empty);

            bool result2 = CertificateValidator.HostnameIsAppropriateForCertificate("subdomain1.subdomain2.SOME.wildcard.host", validCert, out string error2);
            Assert.True(result2, "Good wildcard host should return true");
            Assert.Equal(error2, string.Empty);
        }

        [Fact]
        public void HostnameIsAppropriateForCertificate_BadHost_ReturnsFalseWithAppropriateError()
        {
            bool result = CertificateValidator.HostnameIsAppropriateForCertificate("host_not_in_cert", validCert, out string error);
            Assert.False(result, "Bad simple host should return false");
            Assert.Equal("hostname 'host_not_in_cert' is not appropriate for certificate issued to [localhost;someotherhost;*.some.wildcard.host;leaf_cert]", error);

            bool result2 = CertificateValidator.HostnameIsAppropriateForCertificate("some.wildcard.host", validCert, out string error2);
            Assert.False(result2, "Bad wildcard host should return false");
            Assert.Equal("hostname 'some.wildcard.host' is not appropriate for certificate issued to [localhost;someotherhost;*.some.wildcard.host;leaf_cert]", error2);
        }

        [Fact]
        public void GetAllCertificateHosts_CreateCorrectHostsList()
        {
            List<string> hostnames = CertificateValidator.GetAllCertificateHosts(validCert);
            Assert.Equal(4, hostnames.Count);
            Assert.Contains("leaf_cert", hostnames);
            Assert.Contains("localhost", hostnames);
            Assert.Contains("*.some.wildcard.host", hostnames);
            Assert.Contains("someotherhost", hostnames);
        }

        private X509Chain CreateTestChainForValidCert()
        {
            var ch = new X509Chain();
            ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            ch.ChainPolicy.ExtraStore.Add(subrootCert);
            ch.ChainPolicy.ExtraStore.Add(rootCert);
            ch.Build(validCert);
            return ch;
        }

        private X509Chain CreateTestChainForUntrustedCert()
        {
            var ch = new X509Chain();
            ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            ch.ChainPolicy.ExtraStore.Add(untrustedRootCert);
            ch.Build(untrustedCert);
            return ch;
        }
    }
}
