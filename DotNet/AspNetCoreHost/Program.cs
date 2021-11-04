using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspNetCoreHost
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            string serviceDllPath = args[0];
            int port = int.Parse(args[1]);
            Task runTask = RunWebService(serviceDllPath, port);
            await runTask;
        }

        private static Task RunWebService(string serviceDllPath, int port)
        {
            Assembly a = Assembly.LoadFrom(serviceDllPath);
            Type startupType = a.GetTypes().Where(x => x.Name == "Startup").First();
            IHost host = CreateHostBuilder(startupType, port).Build();
            Task runTask = host.RunAsync();
            return runTask;
        }

        private static IHostBuilder CreateHostBuilder(Type startupType, int port) =>
            Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder
                    .UseUrls($"http://localhost:{port}/")
                    .UseStartup(startupType);
            });
    }
}
