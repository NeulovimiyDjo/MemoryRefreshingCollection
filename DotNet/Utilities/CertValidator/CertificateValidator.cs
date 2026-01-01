using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LinqToDB.Tools;

namespace MyCertValidator
{
    public class CertificateValidator
    {
        private readonly X509CertificateCollection _rootStore;
        private readonly X509RevocationMode _revocationMode;
        private readonly bool _allowRevocationStatusUnknown;

        public CertificateValidator(string trustedRootCertsBundlePath, string revocationModeStr)
        {
            _rootStore = CreateRootStore(trustedRootCertsBundlePath);
            _revocationMode = ParseRevocationMode(revocationModeStr);
            _allowRevocationStatusUnknown = revocationModeStr.ToLower().EndsWith("_allow_status_unknown");
        }

        public bool Validate(X509Certificate cert, X509Chain chain, out string error)
        {
            error = "";
            X509Certificate2 x509Certificate2 = new(cert);

            X509Chain ch = new();
            ch.ChainPolicy.RevocationMode = _revocationMode;
            ch.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            ch.ChainPolicy.CustomTrustStore.Clear();
            ch.ChainPolicy.CustomTrustStore.AddRange(_rootStore);
            ch.ChainPolicy.ExtraStore.Clear();
            if (chain is not null)
            {
                foreach (X509ChainElement chainElement in chain.ChainElements)
                {
                    if (chainElement.Certificate.Thumbprint != x509Certificate2.Thumbprint)
                        ch.ChainPolicy.ExtraStore.Add(chainElement.Certificate);
                }
            }

            bool built = ch.Build(x509Certificate2);
            if (built)
                return true;

            if (_revocationMode.In(X509RevocationMode.Online, X509RevocationMode.Offline)
                && _allowRevocationStatusUnknown
                && ch.ChainStatus.All(x => x.Status.In(
                    X509ChainStatusFlags.RevocationStatusUnknown,
                    X509ChainStatusFlags.OfflineRevocation)))
            {
                _logger.Warn($"Failed to check revocation status for certificate '{cert.Subject}':\n{DisplayErrors(ch)}");
                return true;
            }

            error = DisplayErrors(ch);
            return false;
        }

        public static bool HostnameIsAppropriateForCertificate(string hostname, X509Certificate cert, out string error)
        {
            X509Certificate2 x509Certificate2 = new(cert);

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
                    string wildcardPart = certHn[1..];
                    if (hostname.EndsWith(wildcardPart, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
        }

        public static List<string> GetAllCertificateHosts(X509Certificate cert)
        {
            X509Certificate2 x509Certificate2 = new(cert);
            List<string> hostnames = GetAlternativeDnsNames(x509Certificate2);
            string hostFromSubjectCN = cert.Subject
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .First(x => x.StartsWith("CN="))
                [3..];
            hostnames.Add(hostFromSubjectCN);
            return hostnames;
        }

        private static string DisplayErrors(X509Chain ch)
        {
            List<X509ChainElement> elements = new();
            foreach (X509ChainElement elem in ch.ChainElements)
                elements.Add(elem);

            string chainErrors = ChainStatusToString(ch.ChainStatus);
            string elementErrors = string.Join("\n", elements.Select(x => ChainElementToString(x)));
            return $"ChainErrors:\n{chainErrors}\nChainElements:\n{elementErrors}";

            static string ChainElementToString(X509ChainElement elem) => string.Join(
                "\n",
                "Subject: " + elem.Certificate.Subject,
                "-Issuer: " + elem.Certificate.Issuer,
                "-Errors: " + ChainStatusToString(elem.ChainElementStatus)
            );

            static string ChainStatusToString(X509ChainStatus[] statuses) => string.Join(
                "\n",
                statuses.Select(x => $"{x.StatusInformation.Trim()} ({x.Status})")
            );
        }

        private static X509CertificateCollection CreateRootStore(string certsBundlePath)
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

        private static X509RevocationMode ParseRevocationMode(string modeStr) => modeStr.ToLower() switch
        {
            "online" => X509RevocationMode.Online,
            "online_allow_status_unknown" => X509RevocationMode.Online,
            "offline" => X509RevocationMode.Offline,
            "offline_allow_status_unknown" => X509RevocationMode.Offline,
            "none" => X509RevocationMode.NoCheck,
            _ => throw new Exception($"Invalid revocation mode setting '{modeStr}'"),
        };

        private static List<string> GetAlternativeDnsNames(X509Certificate2 cert)
        {
            const string SAN_OID = "2.5.29.17";

            X509Extension extension = cert.Extensions[SAN_OID];
            if (extension is null)
                return new List<string>();

            // Tag value "2" is defined by:
            //    dNSName                         [2]     IA5String,
            // in: https://datatracker.ietf.org/doc/html/rfc5280#section-4.2.1.6
            Asn1Tag dnsNameTag = new(TagClass.ContextSpecific, tagValue: 2, isConstructed: false);

            AsnReader asnReader = new(extension.RawData, AsnEncodingRules.BER);
            AsnReader sequenceReader = asnReader.ReadSequence(Asn1Tag.Sequence);

            List<string> resultList = new();
            while (sequenceReader.HasData)
            {
                Asn1Tag tag = sequenceReader.PeekTag();
                if (tag != dnsNameTag)
                {
                    sequenceReader.ReadEncodedValue();
                    continue;
                }

                string dnsName = sequenceReader.ReadCharacterString(UniversalTagNumber.IA5String, dnsNameTag);
                resultList.Add(dnsName);
            }
            return resultList;
        }
    }
}
