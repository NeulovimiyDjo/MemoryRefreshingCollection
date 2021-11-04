using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Certs
{
    public class CertificateValidator
    {
        private readonly X509CertificateCollection rootStore;
        private readonly X509RevocationMode revocationMode;

        public CertificateValidator(string trustedRootCertsBundlePath, string revocationModeStr)
        {
            rootStore = CreateRootStore(trustedRootCertsBundlePath);
            revocationMode = ParseRevocationMode(revocationModeStr);
        }

        public bool Validate(X509Certificate cert, X509Chain chain, out string error)
        {
            var x509Certificate2 = new X509Certificate2(cert);

            var ch = new X509Chain();
            ch.ChainPolicy.RevocationMode = revocationMode;
            ch.ChainPolicy.CustomTrustStore.AddRange(rootStore);
            ch.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            if (chain is not null)
            {
                foreach (X509ChainElement chainElement in chain.ChainElements)
                {
                    if (chainElement.Certificate.Thumbprint != x509Certificate2.Thumbprint)
                        ch.ChainPolicy.ExtraStore.Add(chainElement.Certificate);
                }
            }
            bool built = ch.Build(x509Certificate2);

            if (!built)
            {
                var elements = new List<X509ChainElement>();
                foreach (X509ChainElement elem in ch.ChainElements)
                    elements.Add(elem);

                string elementErrors = string.Join(
                    "\n",
                    elements.Select(x => ChainElementToString(x))
                );

                string chainErrors = ChainStatusToString(ch.ChainStatus);

                error = $"ChainErrors:\n{chainErrors}\nChainElements:\n{elementErrors}";

                return false;
            }

            error = "";
            return true;
        }

        public static bool HostnameIsAppropriateForCertificate(string hostname, X509Certificate cert, out string error)
        {
            var x509Certificate2 = new X509Certificate2(cert);

            List<string> certHostnames = GetAllCertificateHosts(x509Certificate2);
            if (certHostnames.Any(certHn => string.Equals(hostname, certHn, StringComparison.OrdinalIgnoreCase) || IsWildcardEqual(hostname, certHn)))
            {
                error = "";
                return true;
            }

            error = $"hostname '{hostname}' is not appropriate for certificate issued to [{string.Join(";", certHostnames)}]";
            return false;

            static bool IsWildcardEqual(string hostname, string certHn)
            {
                if (certHn.StartsWith("*."))
                {
                    string wildcardPart = certHn.Substring(1, certHn.Length - 1);
                    if (hostname.EndsWith(wildcardPart, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
        }

        public static List<string> GetAllCertificateHosts(X509Certificate cert)
        {
            var x509Certificate2 = new X509Certificate2(cert);
            List<string> hostnames = GetAlternativeDnsNames(x509Certificate2);
            string hostFromSubjectCN = cert.Subject
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .First(x => x.StartsWith("CN="))
                .Remove(0, 3);
            hostnames.Add(hostFromSubjectCN);
            return hostnames;
        }

        private string ChainElementToString(X509ChainElement elem)
            => string.Join(
                "\n",
                "Subject: " + elem.Certificate.Subject,
                "-Issuer: " + elem.Certificate.Issuer,
                "-Errors: " + ChainStatusToString(elem.ChainElementStatus)
            );

        private string ChainStatusToString(X509ChainStatus[] statuses)
            => string.Join(
                "\n",
                statuses.Select(x => x.StatusInformation.Trim())
            );

        private X509CertificateCollection CreateRootStore(string certsBundlePath)
        {
            string bundleFileContent = File.ReadAllText(certsBundlePath);
            string[] pemCerts = bundleFileContent
                .Split("-----BEGIN CERTIFICATE-----", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Replace("-----END CERTIFICATE-----", "").Trim())
                .ToArray();
            X509CertificateCollection certCollection = new();
            foreach (string pemCert in pemCerts)
            {
                // Encoding.UTF8.GetBytes("--begin cert--...--end cert--") also works on linux and windows
                X509Certificate2 cert = new(Convert.FromBase64String(pemCert));
                certCollection.Add(cert);
            }
            return certCollection;
        }

        private X509RevocationMode ParseRevocationMode(string revocationModeStr)
        {
            switch (revocationModeStr)
            {
                case "online":
                    return X509RevocationMode.Online;
                case "offline":
                    return X509RevocationMode.Offline;
                default:
                    return X509RevocationMode.NoCheck;
            }
        }

        private static List<string> GetAlternativeDnsNames(X509Certificate2 cert)
        {
            const string SAN_OID = "2.5.29.17";

            var extension = cert.Extensions[SAN_OID];
            if (extension is null)
            {
                return new List<string>();
            }

            // Tag value "2" is defined by:
            //
            //    dNSName                         [2]     IA5String,
            //
            // in: https://datatracker.ietf.org/doc/html/rfc5280#section-4.2.1.6
            var dnsNameTag = new Asn1Tag(TagClass.ContextSpecific, tagValue: 2, isConstructed: false);

            var asnReader = new AsnReader(extension.RawData, AsnEncodingRules.BER);
            var sequenceReader = asnReader.ReadSequence(Asn1Tag.Sequence);

            var resultList = new List<string>();

            while (sequenceReader.HasData)
            {
                var tag = sequenceReader.PeekTag();
                if (tag != dnsNameTag)
                {
                    sequenceReader.ReadEncodedValue();
                    continue;
                }

                var dnsName = sequenceReader.ReadCharacterString(UniversalTagNumber.IA5String, dnsNameTag);
                resultList.Add(dnsName);
            }

            return resultList;
        }
    }
}
