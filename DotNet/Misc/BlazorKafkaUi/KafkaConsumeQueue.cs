namespace BlazorKafkaUi.Models
{
    public class KafkaConsumeQueue : KafkaQueue
    {
        public long LastConsumedOffset { get; set; }
    }
}
