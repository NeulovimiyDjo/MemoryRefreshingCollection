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


        public static XmlSchema Schema
        {
            get
            {
                string xs = "http://www.w3.org/2001/XMLSchema";
                string tns = "http://schemas.microsoft.com/2003/10/Serialization/";

                XmlSchema schema = new XmlSchema();
                schema.TargetNamespace = tns;
                schema.AttributeFormDefault = XmlSchemaForm.Qualified;
                schema.ElementFormDefault = XmlSchemaForm.Qualified;

                XmlSchemaElement dateElem = new XmlSchemaElement();
                dateElem.Name = "date";
                dateElem.SchemaTypeName = new XmlQualifiedName("date", xs);
                dateElem.IsNillable = true;
                schema.Items.Add(dateElem);

                return schema;
            }
        }

        public static XmlQualifiedName MySchema(XmlSchemaSet schemas)
        {
            XmlSchema schema = Schema;
            schemas.Add(schema);

            return new XmlQualifiedName("date", "http://www.w3.org/2001/XMLSchema");
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader) => this.date = reader.ReadElementContentAsDateTime();
        public void WriteXml(XmlWriter writer) => writer.WriteValue(this.date.ToString("yyyy-MM-dd"));

        public DateTime GetDate() => DateTime.SpecifyKind(this.date, DateTimeKind.Utc);
        public new string ToString() => this.date.ToString("yyyy-MM-dd");
    }
}