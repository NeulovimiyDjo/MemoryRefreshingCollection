using System.Collections.Generic;
using BlazorKafkaUi.Models;

namespace BlazorKafkaUi.State
{
    internal static class KafkaQueues
    {
        public static List<KafkaQueue> List { get; private set; } = new();

        static KafkaQueues()
        {
            List.Add(KafkaData.ConsumeQueue1);
            List.Add(KafkaData.ConsumeQueue2);
            List.Add(KafkaData.ProduceQueue1);
        }

        public static List<string> GetMessageTemplateList(string queueName)
        {
            if (queueName == KafkaData.ConsumeQueue1.Name)
                return new() { "Template11" };
            if (queueName == KafkaData.ConsumeQueue2.Name)
                return new() { "Template21", "Template22" };
            throw new($"Invalid queue name '{queueName}' while getting message template list");
        }

        public static string CreateMessageTemplate(string queueName, string templateName)
        {
            string fullTemplateName = $"{queueName}.{templateName}";
            if (fullTemplateName == $"{KafkaData.ConsumeQueue1.Name}.Template11")
                return KafkaData.Template11;
            if (fullTemplateName == $"{KafkaData.ConsumeQueue2.Name}.Template21")
                return KafkaData.Template21;
            if (fullTemplateName == $"{KafkaData.ConsumeQueue21.Name}.Template22")
                return KafkaData.Template22;
            throw new($"Invalid queue and/or template name '{queueName}' while creating message template");
        }
    }
}
