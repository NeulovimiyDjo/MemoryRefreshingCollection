using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SOAP.Simulator.Endpoints;
using SoapCore;
using SoapCore.Extensibility;
using TestWeb.Helpers;

namespace TestWeb
{
    public class Startup
    {
        private readonly List<Type> _soapSimulatorEndpoints;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _soapSimulatorEndpoints = GetSomeServiceTypes();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient("HttpClientWithSSLUntrusted")
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    HttpClientHandler httpClientHandler = new()
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback =
                            (httpRequestMessage, cert, cetChain, policyErrors) =>
                            {
                                return true;
                            }
                    };

                    bool useCertAuth = Configuration.GetSection("AppSettings").GetValue<bool>("UseCertAuth");
                    if (useCertAuth)
                    {
                        string certFilePath = "client_cert.crt";
                        string certKeyPath = "client_cert.key";
                        var cert = X509Certificate2.CreateFromPemFile(certFilePath, certKeyPath);
                        httpClientHandler.ClientCertificates.Add(cert);
                    }

                    return httpClientHandler;
                });

            foreach (Type serviceType in _soapSimulatorEndpoints)
                services.AddTransient(serviceType);
            services.AddMvc();
            services.AddControllers();

            services.AddSingleton<IMessageInspector2, RewriteResponseMessageInspector>();
            services.AddSingleton<IFaultExceptionTransformer, CustomExceptionTransformer>();
            services.AddSoapServiceOperationTuner<InfoPassServiceOperationTuner>();
            services.AddSoapCore();

            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UsePathBase(Configuration.GetSection("AppSettings").GetValue<string>("PathBase"));
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                foreach (Type serviceType in _soapSimulatorEndpoints)
                {
                    endpoints.UseSoapEndpoint(
                        serviceType,
                        $"/SOAP/{GetEndpointName(serviceType)}.svc",
                        new SoapEncoderOptions(),
                        SoapSerializer.XmlSerializer
                    );
                }

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private static string GetEndpointName(Type serviceType)
        {
            return serviceType.Name;
        }
    }
}
