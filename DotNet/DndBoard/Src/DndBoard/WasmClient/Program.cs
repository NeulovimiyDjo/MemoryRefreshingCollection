using System;
using System.Net.Http;
using System.Threading.Tasks;
using DndBoard.ClientCommon;
using DndBoard.ClientCommon.Helpers;
using DndBoard.ClientCommon.Store;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DndBoard.WasmClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            
            builder.Services.AddScoped<BoardRenderer>();
            builder.Services.AddTransient<CanvasMapRenderer>();
            builder.Services.AddTransient<BoardHubManager>();
            builder.Services.AddScoped<AppState>();

            await builder.Build().RunAsync();
        }
    }
}
