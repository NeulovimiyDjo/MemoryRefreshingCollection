using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMonitoring
{
    public abstract class MetricSourceBase : IMetricSource
    {
        public string FullMetricName
        {
            get
            {
                //<library>_<custom_name>(_<unit>)?(_total)?
                return $"{MonitoringSettings.MetricNamePrefix}_{MetricName}" +
                    $"{(MetricUnit != MetricUnitsEnum.None ? "_" + MetricUnit.ToString().ToLower() : "")}" +
                    $"{(MetricType == MetricTypesEnum.Counter ? "_total" : "")}";
            }
        }
        public virtual bool IsEndpoint => false;

        protected abstract string MetricName { get; }
        protected abstract MetricTypesEnum MetricType { get; }
        protected abstract MetricUnitsEnum MetricUnit { get; }
        protected abstract string MetricDescription { get; }
        protected abstract Task<List<MetricSample>> GetMetricSamples(IDbScope dbScope);

        public async Task<string> GetMetricText(
            IDbScope dbScope, Func<IMetricWriter> createMetricWriterFunc)
        {
            List<MetricSample> metricSamples = await GetMetricSamples(dbScope);

            IMetricWriter metricWriter = createMetricWriterFunc();
            if (metricSamples.Any())
                metricWriter.AddMetricInfo(FullMetricName, MetricType, MetricDescription);

            foreach (MetricSample sample in metricSamples)
                await metricWriter.AddMetricSample(FullMetricName, sample.Labels, sample.Value);

            return metricWriter.ToString();
        }
    }
}
