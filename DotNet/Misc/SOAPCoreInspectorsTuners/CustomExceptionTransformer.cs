using SoapCore;
using SoapCore.Extensibility;
using System;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace SOAPCoreService
{
    public class CustomExceptionTransformer : IFaultExceptionTransformer
    {
        public Message ProvideFault(
            Exception exception,
            MessageVersion messageVersion,
            Message requestMessage,
            XmlNamespaceManager xmlNamespaceManager)
        {
            if (exception is HardcodedFaultException)
                return CreateHardcodedFault(exception, messageVersion);
            else
                return CreateDefaultFault(exception, messageVersion, requestMessage, xmlNamespaceManager);
        }

        private static Message CreateHardcodedFault(Exception exception, MessageVersion messageVersion)
        {
            var bytes = Encoding.UTF8.GetBytes(exception.Message);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(bytes, XmlDictionaryReaderQuotas.Max);
            return Message.CreateMessage(reader, int.MaxValue, messageVersion);
        }

        private static Message CreateDefaultFault(Exception exception, MessageVersion messageVersion, Message requestMessage, XmlNamespaceManager xmlNamespaceManager)
        {
            DefaultFaultExceptionTransformer<CustomMessage> defaultExceptionTransformer = new();
            return defaultExceptionTransformer.ProvideFault(exception, messageVersion, requestMessage, xmlNamespaceManager);
        }
    }
}
