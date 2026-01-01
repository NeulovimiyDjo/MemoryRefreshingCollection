using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Configuration;

namespace WCFHostLib.Validators
{
    public class CertificateValidator : ICertificateValidator
    {
        private static readonly string requiredClientCertificateSubject = WebConfigurationManager.AppSettings["RequiredClientCertificateSubject"].Trim().ToLower();
        private static readonly string revocationModeStr = WebConfigurationManager.AppSettings["RequiredClientCertificateRevocationMode"]?.Trim().ToLower();
        private static X509RevocationMode revocationMode
        {
            get
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
        }


        public bool Validate(X509Certificate2 cert, out string error)
        {
            error = "";


            X509Chain ch = new X509Chain();
            ch.ChainPolicy.RevocationMode = revocationMode;
            bool built = ch.Build(cert);

            if (!built)
            {
                var elements = new List<X509ChainElement>();
                foreach (X509ChainElement elem in ch.ChainElements)
                    elements.Add(elem);

                error = string.Join(
                    "\n",
                    elements.Select(x => ChainElementToString(x))
                );

                return false;
            }

            
            if (cert.Subject.Trim().ToLower() == requiredClientCertificateSubject)
                return true;
            else
                error += $"Certificate subject \"{cert.Subject}\" is invalid";


            return false;
        }


        private string ChainElementToString(X509ChainElement elem)
            => string.Join(
                "\n",
                "Subject: " + elem.Certificate.Subject,
                "-IsValid: " + elem.Certificate.Verify(),
                "-ChainStatusInfo: " + ChainStatusToString(elem.ChainElementStatus)
            );

        private string ChainStatusToString(X509ChainStatus[] statuses)
            => string.Join(
                "\n",
                statuses.Select(x => x.StatusInformation.Trim())
            );
    }
}
