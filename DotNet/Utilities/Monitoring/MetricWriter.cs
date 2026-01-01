using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonitoring
{
    public class MetricWriter
    {
        private readonly StringBuilder stringBuilder = new();

        public void AddMetricInfo(
            string name, MetricTypesEnum type, string description)
        {
            if (!string.IsNullOrEmpty(description))
                stringBuilder.Append($"# HELP {name} {description}\n");
            stringBuilder.Append($"# TYPE {name} {type.ToString().ToLower()}\n");
        }

        public async Task AddMetricSample(
            string name, IEnumerable<KeyValuePair<string, object>> labels, object value)
        {
            string labelsStr = string.Join(",", labels.Select(x => $"{x.Key}=\"{Escape(FormatValue(x.Value))}\""));
            stringBuilder.Append($"{name}{{");
            stringBuilder.Append(labelsStr);
            stringBuilder.Append($"}}");
            stringBuilder.Append($" {FormatValue(value)}\n");
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        private static object FormatValue(object value)
        {
            if (value is double doubleV)
                return doubleV.ToString(null, new NumberFormatInfo { NumberDecimalSeparator = MonitoringSettings.NumberDecimalSeparator });
            if (value is decimal decimalV)
                return decimalV.ToString(null, new NumberFormatInfo { NumberDecimalSeparator = MonitoringSettings.NumberDecimalSeparator });
            return value;
        }

        private static string Escape(object value)
        {
            return value?.ToString().Replace("\"", "\\\"");
        }
    }
}
