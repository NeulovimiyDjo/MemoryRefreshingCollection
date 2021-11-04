using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SharedProjects.Encryption
{
    public static class CertificateHelper
    {
        public static X509Certificate2 LoadCertificate(string thumbprint, StoreName storeName, StoreLocation storeLocation)
        {
            thumbprint = Regex.Replace(thumbprint, @"[^\da-fA-F]", string.Empty).ToUpper();


            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                var foundCerts = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (foundCerts.Count != 1)
                {
                    throw new Exception("Failed to find a unique certificate.");
                }

                return foundCerts[0];
            }
        }
    }
}
