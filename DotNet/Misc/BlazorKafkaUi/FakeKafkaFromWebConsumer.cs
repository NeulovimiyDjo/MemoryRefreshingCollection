using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorKafkaUi.Models;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace BlazorKafkaUi.Fakes
{
    public class FakeKafkaFromWebConsumer : FakeConsumerBase
    {
        private static readonly string _baseAddress = "";
        private static readonly HttpClient _httpClient = new();
        private string _topic = null;
        private long _lastConsumedOffset = -1;

        public override void Subscribe(string topic)
        {
            _topic = topic;
        }

        public override List<TopicPartitionOffset> Commit()
        {
            if (_lastConsumedOffset <= 0)
                throw new Exception($"Nothing to commit");
            using HttpRequestMessage httpReq = new(HttpMethod.Post, $"{_baseAddress}/{_topic}");
            httpReq.Content = JsonContent.Create(_lastConsumedOffset);
            using HttpResponseMessage httpRes = _httpClient.SendAsync(httpReq).GetAwaiter().GetResult();
            string responseContent = httpRes.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!httpRes.IsSuccessStatusCode)
                throw new Exception($"Kafka service responded with error: code={(int)httpRes.StatusCode}, content='{responseContent}'");
            return new();
        }

        protected override ConsumeResult<string, string> ConsumeCore()
        {
            using HttpRequestMessage httpReq = new(HttpMethod.Get, $"{_baseAddress}/{_topic}?offset={_lastConsumedOffset}");
            using HttpResponseMessage httpRes = _httpClient.SendAsync(httpReq).GetAwaiter().GetResult();
            if (httpRes.StatusCode == HttpStatusCode.NoContent)
                return null;

            string responseContent = httpRes.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!httpRes.IsSuccessStatusCode)
                throw new Exception($"Kafka service responded with error: code={(int)httpRes.StatusCode}, content='{responseContent}'");

            KafkaMessage message = JsonConvert.DeserializeObject<KafkaMessage>(responseContent);
            _lastConsumedOffset = message.Offset;
            return new ConsumeResult<string, string>
            {
                Topic = message.Topic,
                Partition = message.Partition,
                Offset = message.Offset,
                Message = new Message<string, string>
                {
                    Key = message.MessageKey,
                    Value = message.MessageValue,
                    Timestamp = new Timestamp(DateTime.SpecifyKind(message.MessageTimestamp.Value, DateTimeKind.Utc)),
                },
            };
        }
    }
}
