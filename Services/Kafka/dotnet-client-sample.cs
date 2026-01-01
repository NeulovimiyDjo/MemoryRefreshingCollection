using System.Threading.Tasks;
using Confluent.Kafka;

namespace DotnetClientSampleProject
{
    public static class DotnetClientSampleClass
    {
        public async Task Produce()
        {
            ProducerConfig producerConfig = new()
            {
                BootstrapServers = "localhost:9094",
                ClientId = "test_client_id",
                MessageTimeoutMs = 5000,
                SocketConnectionSetupTimeoutMs = 5000,
                SocketTimeoutMs = 5000,
                RequestTimeoutMs = 5000,
                TransactionTimeoutMs = 5000,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "user1",
                SaslPassword = "111111",
            };

            using IProducer<Null, string> producer = new ProducerBuilder<Null, string>(producerConfig)
                .SetErrorHandler((p, e) =>
                {
                    System.Diagnostics.Debug.Write($"ErrorHandler: {e.Code} {e.Reason}");
                })
                .Build();

            Message<Null, string> message = new();
            message.Value = "test msg";
            string topic = "test_topic"; // don't forget to create topic in kafka-ui (timeout exception otherwise)
            try
            {
                DeliveryResult<Null, string> dr = await producer.ProduceAsync(topic, message);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Write($"Ex: {ex.Message}");
            }

            return;
        }
    }
}
