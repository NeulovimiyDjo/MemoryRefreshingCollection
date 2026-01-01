using System;

namespace BlazorKafkaUi.Models
{
    public class KafkaMessage
    {
        public string Topic { get; set; }
        public int Partition { get; set; }
        public long Offset { get; set; }
        public string MessageKey { get; set; }
        public string MessageValue { get; set; }
        public DateTime MessageTimestamp { get; set; }
    }
}
