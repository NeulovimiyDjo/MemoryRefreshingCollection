using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Amqp.Types;
using NLog;

namespace DotnetClientSampleProject
{
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const bool SendUsingAmqpNetLite = true;
        private const bool ReceiveUsingAmqpNetLite = true;

        public static async Task Main()
        {
            logger.Trace("Executing using AmqpNetLite");
            Address address = new("localhost", 61616, "artemis", "artemis", "/", "AMQP");
            ConnectionFactory connectionFactory = Connection.Factory;
            connectionFactory.AMQP.ContainerId = "mysrv:" + "artemis" + ":" + DateTime.Now.ToString("yyyyMMddHHmmssfffffff"); // system:user:uniqueid
            Connection connection = await connectionFactory.CreateAsync(address);
            Session session = new(connection);

            if (SendUsingAmqpNetLite)
            {
                Target target = new()
                {
                    Address = "test.dest",
                    Capabilities = new Symbol[] { new("queue") }, // Auto-creation of queues
                };
                target.Durable = 0u;
                target.Dynamic = false;

                Source source = new();
                source.Address = $"ID:{Dns.GetHostName()}:{Guid.NewGuid().ToString()}:1:1:1";
                source.Outcomes = new Symbol[]
                {
                    new Accepted().Descriptor.Name,
                    new Rejected().Descriptor.Name,
                };

                Attach attach = new()
                {
                    Source = source,
                    Target = target,
                    SndSettleMode = SenderSettleMode.Unsettled,
                    IncompleteUnsettled = false,
                    InitialDeliveryCount = 0u,
                };

                string name = source.Address + ":" + target.Address;
                SenderLink sender = new(session, name, attach, null);

                Message message = new("JsonConvert.SerializeSomething");
                message.Header = new();
                message.Header.Priority = Convert.ToByte(5);
                message.Header.Durable = false;

                message.Properties = new();
                message.Properties.CreationTime = DateTime.UtcNow;
                message.Properties.SetMessageId(source.Address + "-1");
                message.Properties.SetCorrelationId(Guid.NewGuid().ToString());
                message.Properties.To = "test.dest";
                message.Properties.Subject = "3";

                message.ApplicationProperties = new();
                message.ApplicationProperties["MSGUID"] = Guid.NewGuid().ToString("N");
                message.ApplicationProperties["MyServiceName"] = "AAbbb123";

                message.MessageAnnotations = new();
                message.MessageAnnotations[new Symbol("x-opt-jms-msg-type")] = new sbyte?(5); // Text message
                //message.MessageAnnotations[new Symbol("x-opt-routing-type")] = (sbyte)1; // Anycast=1 Multicast=0 Both=null

                await sender.SendAsync(message);
                await sender.CloseAsync();
            }

            if (ReceiveUsingAmqpNetLite)
            {
                Source source = new()
                {
                    Address = "test.dest",
                    Capabilities = new Symbol[] { new("queue") }, // Auto-creation of queues
                };
                ReceiverLink receiver = new(session, "my-receiver-link", source, null);

                Message message = await receiver.ReceiveAsync();
                logger.Trace($"MessageId {message.Properties.MessageId}");
                logger.Trace($"To: {message.Properties.To}");
                logger.Trace($"MyServiceName: {message.ApplicationProperties.Map.GetValueOrDefault("MyServiceName")}");
                logger.Trace($"Priority {message.Header.Priority}");
                logger.Trace($"Body: {message.Body}");

                receiver.Accept(message);
                await receiver.CloseAsync();
            }

            await session.CloseAsync();
            await connection.CloseAsync();
        }
    }
}
