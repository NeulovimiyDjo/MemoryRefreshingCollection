using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace WSDLTool
{
    public static class EnvelopeCreator
    {
        public static string CreateMyRequestEnvelope()
        {
            var env = new MyRequest.Envelope
            {
                Header = new MyRequest.Header(),
                Body = new MyRequest.Body()
                {
                    MyRequest = new MyRequest
                    {
                        Id = "o0",
                        Root = 1,
                        SomeData = "value value xx zz"
                    },
                },
            };
            var serializer = new XmlSerializer(typeof(MyRequest.Envelope));
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = true,
            };
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, settings))
            {
                serializer.Serialize(writer, env, env.xmlns);
            }
            return builder.ToString();
        }
    }
}