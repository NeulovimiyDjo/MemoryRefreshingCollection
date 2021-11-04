using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Xml;

namespace WSDLTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string wsdl = GetWsdlFromService<DoSomethingService>();
            File.WriteAllText("out.wsdl", wsdl);

            string serviceName = "out";
            string arg = $"/noConfig /namespace:*,{serviceName}Namespace /out:I{serviceName}.cs {serviceName}.wsdl";
            Process.Start("svcutil.exe", arg);

            string xml = EnvelopeCreator.CreateMyRequestEnvelope();
            File.WriteAllText("out.xml", xml);

            TestDataContractSerializer();
        }

        private static void TestDataContractSerializer()
        {
            var Mark = new Person()
            {
                FirstName = "Mark",
                LastName = "mark@example.com",
                ID = 2
            };

            var serializer = new DataContractSerializer(typeof(Person));
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t"
            };
            using var writer = XmlWriter.Create(Console.Out, settings);
            serializer.WriteObject(writer, Mark);
        }
    }
}
