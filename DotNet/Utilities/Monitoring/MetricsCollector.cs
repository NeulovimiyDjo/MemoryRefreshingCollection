using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Common;
using Unity;

namespace MyMonitoring
{
    public static class MetricsCollector
    {
        private static string LoggerName => $"{nameof(MetricsCollector)}";

        public static async Task<string> GetAllMetrics(
            IUnityContainer unityContainer, bool isEndpoint)
        {
            IEnumerable<IMetricSource> sources = unityContainer.ResolveAll<IMetricSource>().Where(ms => ms.IsEndpoint == isEndpoint);
            List<string> metrics = new();
            foreach (IMetricSource source in sources)
            {
                try
                {
                    Loggers.Monitoring.Trace($"{LoggerName}: Generating metric '{source.FullMetricName}'");
                    string metricText = await source.GetMetricText(dbScope, () => new MetricWriter()));
                    metrics.Add(metricText);
                    Loggers.Monitoring.Trace($"{LoggerName}: Generated metric '{source.FullMetricName}'");

                }
                catch (Exception ex)
                {
                    Loggers.Monitoring.LogException($"{LoggerName}: Error generating metric '{source.FullMetricName}':", ex);
                }
            }
            return string.Join("\n", metrics.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
