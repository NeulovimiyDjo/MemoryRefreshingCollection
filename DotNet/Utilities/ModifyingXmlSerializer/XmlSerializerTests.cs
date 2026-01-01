using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Xunit;

namespace XmlSerializerTests
{
    public class XmlSerializerTests
    {
        [Theory]
        [InlineData(typeof(TestRootType), "-filled")]
        [InlineData(typeof(TestRootType), "-emptynodes")]
        [InlineData(typeof(TestRootType), "-nullnodes")]
        public void CreateDisplayText_Produces_ExpectedOutput(Type objType, string subCase)
        {
            object originalObj = XmlSerializerTestsData.CaseSamples[$"{objType.Name}{subCase}"];

            string actualDisplayText = new MyXmlSerializer().Serialize(originalObj);

            string expectedDisplayText = ReadFromFile($"{objType.Name}{subCase}.xml.expected");
            Assert.Equal(expectedDisplayText, actualDisplayText);
        }

        [Theory]
        [InlineData(typeof(TestRootType), "-filled")]
        [InlineData(typeof(TestRootType), "-emptynodes")]
        [InlineData(typeof(TestRootType), "-nullnodes")]
        public void CreateDisplayText_Produces_Result_EquivalentTo_StandardSerialize(Type objType, string subCase)
        {
            object originalObj = XmlSerializerTestsData.CaseSamples[$"{objType.Name}{subCase}"];

            string actualDisplayText = new MyXmlSerializer().Serialize(originalObj);

            string reserializedObject = Serialize(originalObj);
            string normalizedReserializedObject = reserializedObject
                .Replace("string>", "String>");
            Assert.Equal(normalizedReserializedObject, actualDisplayText);
        }

        private static string Serialize(object obj)
        {
            using StringWriter stringWriter = new();
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");
            XmlWriterSettings xmlWriterSettings = new() { OmitXmlDeclaration = true, Indent = true };
            using (XmlWriter writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                new XmlSerializer(obj.GetType()).Serialize(writer, obj, ns);
            }
            return stringWriter.ToString().Replace("\r\n", "\n").Trim();
        }

        private static string ReadFromFile(string fileName)
        {
            string filePath = $"TestData/{fileName}";
            return File.ReadAllText(filePath).Replace("\r\n", "\n").Trim();
        }
    }
}
