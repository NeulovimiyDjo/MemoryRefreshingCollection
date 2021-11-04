using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SomeProject
{
    [XmlSchemaProvider("MySchema")]
    public class Date : IXmlSerializable
    {
        private DateTime date;


        public Date() : this(DateTime.MinValue) { }
        public Date(DateTime date) => this.date = date;

        public static XmlQualifiedName MySchema(XmlSchemaSet xs) => new XmlQualifiedName("date", "http://www.w3.org/2001/XMLSchema");
        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader) => this.date = reader.ReadElementContentAsDateTime();
        public void WriteXml(XmlWriter writer) => writer.WriteValue(this.date.ToString("yyyy-MM-dd"));

        public DateTime GetDate() => this.date;
        public new string ToString() => this.date.ToString("yyyy-MM-dd");
    }
}