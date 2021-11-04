using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CertLoadTestService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = CreateHostBuilder(args.Where(arg => arg != "--console").ToArray());

            if (isService)
                builder.UseWindowsService();

            var host = builder.Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    WorkerOptions options = configuration.GetSection("WorkerOptions").Get<WorkerOptions>();
                    services.AddSingleton(options);
                    services.AddHostedService<Worker>();
                });
    }
}
