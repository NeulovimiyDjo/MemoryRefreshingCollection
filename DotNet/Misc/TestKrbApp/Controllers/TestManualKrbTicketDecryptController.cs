using Kerberos.NET;
using Kerberos.NET.Crypto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TestApp.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class TestManualKrbTicketDecryptController : ControllerBase
   {
       private readonly ILogger<TestAuthController> _logger;

       public TestManualKrbTicketDecryptController(ILogger<TestAuthController> logger)
       {
           _logger = logger;
       }

       [HttpGet]
       [Route(nameof(GetAuthorize))]
       public async Task<IActionResult> GetAuthorize()
       {
           // When Authorization header is present ".AddNegotiate() added in Startup" will still handle it first
           string authorization = Request.Headers[HeaderNames.Authorization];
           if (string.IsNullOrEmpty(authorization))
           {
               Response.Headers["WWW-Authenticate"] = "Negotiate";
               return Unauthorized();
           }

           KerberosAuthenticator authenticator = new(
               new KeyTable(System.IO.File.ReadAllBytes("/app/service.keytab")));

           ClaimsIdentity identity = await authenticator.Authenticate(authorization);
           string name = identity.Name;
           return Content(name);
       }
   }
}
