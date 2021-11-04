using System.IO;
using System.Xml.Serialization;

namespace WCFServices
{
    public static class CommonFunctions
    {
        public static string SerializeAsXmlString(object obj)
        {
            var messageType = obj.GetType();
            var serializer = new XmlSerializer(messageType);
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }
    }
}
