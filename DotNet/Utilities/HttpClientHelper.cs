using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HttpClientHelperProject
{
    public class HttpClientHelper
    {
        private readonly CertificateValidator _certValidator;

        public HttpClient CreateHttpClientHelper(
			long? timeoutSeconds = 60,
            IDictionary<string, string> defaultRequestHeaders = null,
            X509Certificate2 clientCert = null,
            List<string> additionalTrustedHostNames = null)
        {
            _certValidator = new();

			HttpMessageHandler handler = CreateHttpHandler(clientCert, additionalTrustedHostNames);
			HttpClient client = new(handler);

			if (timeoutSeconds.HasValue)
				client.Timeout = TimeSpan.FromSeconds(timeoutSeconds.Value);

			if (defaultRequestHeaders is not null)
			{
				foreach (string key in defaultRequestHeaders.Keys)
					client.DefaultRequestHeaders.Add(key, defaultRequestHeaders[key]);
			}
			
			return client;
        }

        private HttpMessageHandler CreateHttpHandler(
            X509Certificate2 clientCert,
            List<string> additionalTrustedHostNames)
        {
            SocketsHttpHandler handler = new()
            {
                SslOptions = new()
                {
                    RemoteCertificateValidationCallback = ValidateCert,
                },
                PooledConnectionLifetime = TimeSpan.FromMinutes(30),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(60)
            };

            if (clientCert != null)
            {
                handler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) =>
                {
                    return clientCert;
                };
            }

            return handler;

            bool ValidateCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
            {
                if (!_certValidator.Validate(cert, chain, out string validationError))
                {
                    logger.Warn($"Server certificate is not valid:\n{validationError}");
                    return false;
                }

                var reqSslStream = sender as SslStream;
                string requestHostname = reqSslStream != null ? reqSslStream.TargetHostName : $"{sender.GetType()}:{sender}";

                if (!CertificateValidator.HostnameIsAppropriateForCertificate(requestHostname, cert, out string hostnameError))
                {
                    if (additionalTrustedHostNames is not null && additionalTrustedHostNames.Count > 0)
                    {
                        bool anyAdditionalTrustedHostNameIsAppropriateForCertificate = additionalTrustedHostNames.Any(
                            hn => CertificateValidator.HostnameIsAppropriateForCertificate(hn, cert, out string _));
                        if (!anyAdditionalTrustedHostNameIsAppropriateForCertificate)
                        {
                            logger.Warn($"Server certificate hostnames are not appropriate:\n{hostnameError}"
                                + $"\nAdditional trusted host names '{string.Join(";", additionalTrustedHostNames)}' check also failed");
                            return false;
                        }
                    }
                    else
                    {
                        logger.Warn($"Server certificate hostnames are not appropriate:\n{hostnameError}");
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
