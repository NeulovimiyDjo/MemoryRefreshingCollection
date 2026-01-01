using System.Collections.Generic;

namespace BlazorKafkaUi.Models
{
    public abstract class KafkaQueue
    {
        public string Name { get; set; }
        public Queue<KafkaMessage> Messages { get; set; }
        public long LastOffset { get; set; }
        public object Lock { get; set; }
    }
}
