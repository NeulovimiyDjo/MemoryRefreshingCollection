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
using SOAP.Integration.Endpoints;
using SOAP.Simulator.Endpoints;
using SoapCore.Extensibility;
using System.ServiceModel.Channels;

namespace SOAPCoreService
{
    public class Startup
    {
        private List<Type> _endpointTypes;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _endpointTypes = ServicesLocator
                .LocateServices(new Assembly[]
                {
                    Assembly.LoadFrom(@$"{dir}\Endpoints.dll"),
                }).ToList();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IMessageInspector, RewriteResponseMessageInspector>();
            services.AddSingleton<IFaultExceptionTransformer, CustomExceptionTransformer>();
            services.AddSoapServiceOperationTuner<InfoPassServiceOperationTuner>();
            services.AddSoapCore();

            foreach (Type serviceType in _endpointTypes)
                services.AddTransient(serviceType);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                foreach (Type serviceType in _endpointTypes)
                    endpoints.UseSoapEndpoint(
                        serviceType,
                        $"/SOAPEndpoints/{serviceType.Name}.svc",
                        new BasicHttpBinding(),
                        serializer: ChooseSerializer(serviceType)
                    );
            });
        }

        private static SoapSerializer ChooseSerializer(Type serviceType)
        {
            if (serviceType.Assembly.GetName().Name == typeof(SomeType).Assembly.GetName().Name)
                return SoapSerializer.DataContractSerializer;
           return SoapSerializer.XmlSerializer;
        }
    }
}
