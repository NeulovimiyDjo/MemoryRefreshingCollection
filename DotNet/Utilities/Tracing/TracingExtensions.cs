using OpenTelemetry;
using OpenTelemetry.Trace;

namespace MyTracing
{
    public static class TracingExtensions
    {
        public static TracerProviderBuilder AddToFileExporter(
            this TracerProviderBuilder builder,
            string outFilePath)
        {
            return builder.AddProcessor(
                new SimpleActivityExportProcessor(
                    new ToFileActivityExporter(outFilePath)));
        }
    }
}
