using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CertLoadTestService
{
    public class Worker : BackgroundService
    {
        private readonly WorkerOptions _options;

        public Worker(ILogger<Worker> logger, WorkerOptions options)
        {
            _options = options;
            OutputWriter.Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var privateCert = GetCertificateFromStore(_options.CertName, StoreLocation.LocalMachine);
                    OutputWriter.Write("Success");
                }
                catch (Exception ex)
                {
                    OutputWriter.Write(ex.Message);
                }
                await Task.Delay(3000,stoppingToken);
            }
        }

        private static X509Certificate2 GetCertificateFromStore(string certName, StoreLocation location)
        {
            X509Store store = new X509Store(location);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection signingCert = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
                if (signingCert.Count == 0)
                    throw new Exception($"Certificate {certName} was not found.");
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}
