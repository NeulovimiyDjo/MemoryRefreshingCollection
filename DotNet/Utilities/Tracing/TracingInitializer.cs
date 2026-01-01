using System;
using System.Collections.Generic;
using System.Net;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MyTracing
{
    public class TracingInitializer : IDisposable
    {
        private static string LoggerName => $"{nameof(TracingInitializer)}";

        private readonly string _serviceName;
        private readonly IDisposable _tracerProvider;

        public TracingInitializer(Type serviceType)
        {
            _serviceName = serviceType.Name;
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(
                            "my_service_name",
                            "my_service_namespace",
                            null,
                            true,
                            null)
                        .AddAttributes(
                            new Dictionary<string, object>
                            {
                                ["host_name"] = Dns.GetHostName(),
                                ["service_name"] = _serviceName,
                            }))
                .AddSource("my_service_activitysource")
                .AddLegacySource("Microsoft.AspNetCore.Hosting.HttpRequestIn")
                .AddProcessor(new DropNonWhitelistedByRootActivityProcessor(new[] {
                    "create_some_operation",
                    "process_some_operation" }))
                .AddOtlpExporter(x => x.Endpoint = new("http://host.docker.internal:18889"));
                .AddToFileExporter("./tracing_output.txt");
                .Build();

            Loggers.Tracing.Debug($"{LoggerName}: Started for service '{_serviceName}'");
        }

        public void Dispose()
        {
            _tracerProvider.Dispose();
            Loggers.Tracing.Debug($"{LoggerName}: Completed for service '{_serviceName}'");
        }
    }
}
