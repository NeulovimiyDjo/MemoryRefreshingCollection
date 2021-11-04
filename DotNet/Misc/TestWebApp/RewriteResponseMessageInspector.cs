using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SoapCore.Extensibility;
using SoapCore.ServiceModel;

namespace TestWeb
{
    public class RewriteResponseMessageInspector : IMessageInspector2
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RewriteResponseMessageInspector(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public object AfterReceiveRequest(ref Message message, ServiceDescription serviceDescription)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, ServiceDescription serviceDescription, object correlationState)
        {
            IHeaderDictionary requestHeaders = _httpContextAccessor.HttpContext.Request.Headers;
            if (requestHeaders.TryGetValue("HardcodedReply", out StringValues messageContent))
            {
                reply = CreateHardcodedMessage(messageContent, reply.Version);
                return;
            }

            if (requestHeaders.ContainsKey("DontWrapResponse"))
                reply = CreateSameMessageWithEmptyBody(reply);
        }

        private Message CreateHardcodedMessage(string messageContent, MessageVersion messageVersion)
        {
            var bytes = Encoding.UTF8.GetBytes(messageContent);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(bytes, XmlDictionaryReaderQuotas.Max);
            return Message.CreateMessage(reader, int.MaxValue, messageVersion);
        }

        private Message CreateSameMessageWithEmptyBody(Message oldMessage)
        {
            Message newMessage = Message.CreateMessage(oldMessage.Version, null);
            newMessage.Headers.CopyHeadersFrom(oldMessage);
            newMessage.Properties.CopyProperties(oldMessage.Properties);
            return newMessage;
        }
    }
}
