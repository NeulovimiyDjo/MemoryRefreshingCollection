using System;
using System.Net.Http;
using System.Net.Http.Json;
using Confluent.Kafka;

namespace BlazorKafkaUi.Fakes
{
    public class FakeKafkaToWebProducer : FakeProducerBase
    {
        private static readonly string _baseAddress = "";
        private static readonly HttpClient _httpClient = new();

        protected override DeliveryResult<Null, string> ProduceCore(string topic, Message<Null, string> message)
        {
            using HttpRequestMessage httpReq = new(HttpMethod.Post, $"{_baseAddress}/{topic}");
            httpReq.Content = JsonContent.Create(message.Value);
            using HttpResponseMessage httpRes = _httpClient.SendAsync(httpReq).GetAwaiter().GetResult();
            string responseContent = httpRes.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!httpRes.IsSuccessStatusCode)
                throw new Exception($"Kafka service responded with error: code={(int)httpRes.StatusCode}, content='{responseContent}'");

            return new DeliveryResult<Null, string>()
            {
                Status = PersistenceStatus.Persisted,
            };
        }
    }
}
