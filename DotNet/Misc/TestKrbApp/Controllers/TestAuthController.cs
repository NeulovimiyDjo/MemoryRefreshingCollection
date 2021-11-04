using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TestAuthController : ControllerBase
    {
        private readonly ILogger<TestAuthController> _logger;

        public TestAuthController(ILogger<TestAuthController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(GetAnonymous))]
        public IEnumerable<WeatherForecast> GetAnonymous() => CreateTestResponse();

        [HttpGet]
        [Route(nameof(GetAuthorize))]
        public IEnumerable<WeatherForecast> GetAuthorize() => CreateTestResponse();

        private static IEnumerable<WeatherForecast> CreateTestResponse()
        {
            string[] Summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
