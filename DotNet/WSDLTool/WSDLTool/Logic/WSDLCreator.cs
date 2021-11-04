using SoapCore;
using SoapCore.Meta;
using SoapCore.ServiceModel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Xml;

namespace WSDLTool.Logic
{
    public static class WSDLCreator
    {
        public static string GetWSDLFromService(string serviceName, string assemblyPath)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            Type serviceType = ServicesLocator.LocateServices(assembly)
                .First(x => x.Name == serviceName);
            return GetWSDLFromType(serviceType);
        }

        private static string GetWSDLFromType(Type serviceType)
        {
            var serviceDescription = new ServiceDescription(serviceType);
            var xmlNamespaceManager = Namespaces.CreateDefaultXmlNamespaceManager();
            //var bodyWriter = new XmlSerializerBodyWriter(service, null, xmlNamespaceManager);
            var bodyWriter = new DataContractBodyWriter(serviceDescription, null);
            var message = Message.CreateMessage(MessageVersion.Soap11, null, bodyWriter);
            message = new MetaMessage(message, serviceDescription, null, xmlNamespaceManager);

            using var memoryStream = new MemoryStream();
            var settings = new XmlWriterSettings { Indent = true };
            using var xmlWriter = XmlWriter.Create(memoryStream, settings);
            WriteXml(message, xmlWriter);
            memoryStream.Position = 0;

            using var streamReader = new StreamReader(memoryStream);
            var result = streamReader.ReadToEnd();
            return result;
        }

        private static void WriteXml(Message message, XmlWriter xmlWriter)
        {
            message.WriteMessage(xmlWriter);
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
        }
    }
}
