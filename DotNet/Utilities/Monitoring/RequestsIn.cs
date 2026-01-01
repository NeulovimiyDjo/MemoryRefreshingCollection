using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyMonitoring
{
    public class RequestsIn : MetricSourceBase
    {
        protected override string MetricName => "requests_in";
        protected override MetricTypesEnum MetricType => MetricTypesEnum.Counter;
        protected override MetricUnitsEnum MetricUnit => MetricUnitsEnum.None;
        protected override string MetricDescription => "Counter for all incoming requests.";
        public override bool IsEndpoint => true;

        private static volatile string lastValue = "0";

        protected override async Task<List<MetricSample>> GetMetricSamples(IDbScope dbScope)
        {
            const string NginxReqCountFilePath = "nginxwc/current.txt";
            if (!System.IO.File.Exists(NginxReqCountFilePath))
                throw new Exception($"Nginx req count file not found.");

            string nginxReqCountFileContent = System.IO.File.ReadAllText(NginxReqCountFilePath).Trim();
            string metricValue = string.IsNullOrEmpty(nginxReqCountFileContent)
                ? lastValue
                : long.Parse(nginxReqCountFileContent).ToString();
            lastValue = metricValue;

            Dictionary<string, object> labels = new();
            return new List<MetricSample>()
            {
                new MetricSample(labels, metricValue),
            };
        }
    }
}
