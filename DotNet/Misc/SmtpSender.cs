using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using NLog;
using MailKitSmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SmtpSender
{
    public static class Program
    {
        public static async Task Main()
        {
            MailKitSmtpClient? client = null;
            try
            {
                client = new MailKitSmtpClient { ServerCertificateValidationCallback = ValidateCert };
                if (timeout > 0)
                    client.Timeout = timeout;

                SecureSocketOptions secureSocketOptions = enableSsl
                    ? port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;
                await client.ConnectAsync(host, port, secureSocketOptions, cancellationToken);

                if ((client.Capabilities & SmtpCapabilities.Authentication) != 0
                    && (defaultCredentials || !string.IsNullOrEmpty(userName)))
                {
                    ICredentials credentials = defaultCredentials
                        ? CredentialCache.DefaultCredentials
                        : string.IsNullOrEmpty(clientDomain)
                            ? new NetworkCredential(userName, password)
                            : new NetworkCredential(userName, password, clientDomain);

                    await client.AuthenticateAsync(credentials, cancellationToken);
                }

                client.XXXXXXXXXX();
            }
            finally
            {
                if (client is not null)
                {
                    if (client.IsConnected)
                        await client.DisconnectAsync(true, cancellationToken);
                    client.Dispose();
                }
            }
        }
    }
}
