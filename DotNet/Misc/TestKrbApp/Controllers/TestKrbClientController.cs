using Kerberos.NET.Client;
using Kerberos.NET.Configuration;
using Kerberos.NET.Credentials;
using Kerberos.NET.Crypto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class TestKrbClientController : ControllerBase
   {
       private readonly ILoggerFactory _loggerFactory;
       private readonly IConfiguration _config;

       public TestKrbClientController(ILoggerFactory loggerFactory, IConfiguration config)
       {
           _loggerFactory = loggerFactory;
           _config = config;
       }

       [HttpGet]
       [Route(nameof(TestClient))]
       public async Task<IActionResult> TestClient([FromQuery] string queryHost, [FromQuery] string pwdFile)
       {
           string host = _config.GetValue<string>("TestClientHost");
           string user = _config.GetValue<string>("TestClientUser");

           if (!string.IsNullOrEmpty(queryHost))
               host = queryHost;

           KerberosCredential krbCred;
           if (!string.IsNullOrEmpty(pwdFile))
               krbCred = new KerberosPasswordCredential(user, System.IO.File.ReadAllText(pwdFile).Trim());
           else
               krbCred = new KeytabCredential(user, new KeyTable(System.IO.File.ReadAllBytes("/app/client.keytab")));

           Krb5Config config = Krb5ConfigurationSerializer.Deserialize(
               System.IO.File.ReadAllText("/etc/krb5.conf")).ToConfigObject();
           KerberosClient krbClient = new(config, _loggerFactory);
           await krbClient.Authenticate(krbCred);
           var krbTicket = await krbClient.GetServiceTicket($"http/{host}");

           using HttpClient httpClient = new();
           httpClient.DefaultRequestHeaders.Add("Authorization", "Negotiate " + Convert.ToBase64String(krbTicket.EncodeGssApi().ToArray()));
           var httpResponse = await httpClient.GetAsync($"http://{host}/TestManualKrbTicketDecrypt/GetAuthorize");
           string responseBody = await httpResponse.Content.ReadAsStringAsync();
           return Content($"Status: {httpResponse.StatusCode}\nBody:\n{responseBody}");
       }
   }
}
