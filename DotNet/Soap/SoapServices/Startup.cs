using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoapCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using WCFServices;

namespace SoapServices
{
    public class Startup
    {
        private List<Type> _servicesTypes;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _servicesTypes = ServicesLocator
                .LocateServices(new Assembly[]
                {
                    Assembly.LoadFrom(@$"{dir}\WCFServices.dll"),
                }).ToList();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IClientFactory>(c => new TestClientFactory());
            services.AddSoapCore();
            foreach (Type serviceType in _servicesTypes)
                services.AddTransient(serviceType);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                foreach (Type serviceType in _servicesTypes)
                    endpoints.UseSoapEndpoint(
                        serviceType,
                        $"/{serviceType.Name}.svc",
                        new BasicHttpBinding(),
                        serializer: ChooseSerializer(serviceType)
                    );
            });
        }

        private static SoapSerializer ChooseSerializer(Type serviceType)
        {
            if (serviceType.Assembly.GetName().Name == "WCFServices")
                return SoapSerializer.DataContractSerializer;
           return SoapSerializer.XmlSerializer;
        }
    }
}
