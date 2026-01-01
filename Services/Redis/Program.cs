//<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.3" />
//<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.3" />
//<PackageReference Include="StackExchange.Redis" Version="2.8.31" />
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Security.Cryptography.X509Certificates;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .ClearProviders()
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter(typeof(Program).FullName, LogLevel.Debug)
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});
var logger = loggerFactory.CreateLogger<Program>();

string connectionString =
    "host.docker.internal:26379,host.docker.internal:26380,host.docker.internal:26381," +
    "serviceName=mymaster,user=x_client,password=x_clientpass";
using var connection = ConnectionMultiplexer.Connect(connectionString, x => {
    x.LoggerFactory = loggerFactory;
    x.Ssl = true;
    x.CertificateValidation += (sender, certificate, chain, sslPolicyErrors) =>
    {
        logger.LogDebug($"Validating cerver cert, subj='{certificate?.Subject}'");
        return true;
    };
    x.CertificateSelection += (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) =>
    {
        logger.LogDebug($"Selecting client cert for host='{targetHost}'");
        var pemCert = X509Certificate2.CreateFromPemFile("client.crt", "client.key");
        X509Certificate2 pfxCert = new(pemCert.Export(X509ContentType.Pfx));
        return pfxCert;
    };
});
IDatabase? rdb = connection.GetDatabase();

long i = 0;
while (i++ < long.MaxValue)
{
    try
    {
        string? oldVal = await rdb.StringGetAsync("TestKey");
        await rdb.StringSetAsync("TestKey", i.ToString());
        logger.LogWarning($"Replaced old val '{oldVal}' with new val '{i}'");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Error accessing redis db");
    }
    await Task.Delay(2_000);
}
