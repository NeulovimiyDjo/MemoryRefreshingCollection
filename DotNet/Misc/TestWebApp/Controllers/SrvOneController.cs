using System;
using Microsoft.AspNetCore.Mvc;

namespace TestWeb.Controllers
{
    [Controller]
    [Route("SrvOne")]
    public class SrvOneController : Controller
    {
        [HttpPost("auth/gettoken")]
        [Produces(typeof(SrvOneResponse))]
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult<SrvOneResponse> GetToken([FromForm] SrvOneRequest srvOneRequest)
        {
            if (string.IsNullOrEmpty(srvOneRequest.user) || string.IsNullOrEmpty(srvOneRequest.password))
                return BadRequest($"Invalid credentials");

            SrvOneResponse srvOneResponse = new()
            {
                token = Guid.NewGuid().ToString(),
            };
            return Ok(srvOneResponse);
        }

        public class SrvOneRequest
        {
            public string user { get; set; }
            public string password { get; set; }
        }

        public class SrvOneResponse
        {
            public string token { get; set; }
        }
    }
}
