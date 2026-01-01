using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;

namespace MyXmlSerializerProject
{
    public class MyXmlSerializer
    {
        private readonly Stack<string> _currentMemberPathStack = new();
        private IClassSettings _currentlySerializedObjectSettings;

        public string Serialize(object obj)
        {
            _currentMemberPathStack.Clear();
            _currentlySerializedObjectSettings = null;
            if (ClassesSettings.Settings.ContainsKey(obj.GetType()))
                _currentlySerializedObjectSettings = ClassesSettings.Settings[obj.GetType()];

            using StringWriter stringWriter = new();
            XmlWriterSettings xmlWriterSettings = new() { OmitXmlDeclaration = true, Indent = true };
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                xmlWriter.WriteStartElement(GetElemNameFromObjectType(obj));
                WriteObjectMembers(obj, xmlWriter);
                xmlWriter.WriteEndElement();
            }
            return stringWriter.ToString().Replace("\r\n", "\n").Trim();
        }

        private void WriteObjectMembers(object obj, XmlWriter xmlWriter)
        {
            IEnumerable<PropertyInfo> props = obj.GetType().GetProperties().Where(x => IsXmlMember(x)).OrderBy(x => x.MetadataToken);
            foreach (PropertyInfo propInfo in props)
            {
                object propValue = propInfo.GetValue(obj);
                WriteMemberIfNotNull(propInfo, propValue, xmlWriter);
            }

            IEnumerable<FieldInfo> fields = obj.GetType().GetFields().Where(x => IsXmlMember(x)).OrderBy(x => x.MetadataToken);
            foreach (FieldInfo fieldInfo in fields)
            {
                object fieldValue = fieldInfo.GetValue(obj);
                WriteMemberIfNotNull(fieldInfo, fieldValue, xmlWriter);
            }
        }

        private void WriteMemberIfNotNull(MemberInfo memberInfo, object memberValue, XmlWriter xmlWriter)
        {
            _currentMemberPathStack.Push(memberInfo.Name);
            if (memberValue is not null && memberValue is not string && memberValue is IList list)
            {
                if (IsXmlArray(memberInfo))
                {
                    xmlWriter.WriteStartElement(GetElemNameFromMemberInfo(memberInfo));
                    WriteArrayItems(GetArrayItemNameFromMemberInfo(memberInfo), list, xmlWriter);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    WriteArrayItems(GetElemNameFromMemberInfo(memberInfo), list, xmlWriter);
                }
            }
            else
            {
                WriteObjectIfNotNull(GetElemNameFromMemberInfo(memberInfo), memberValue, xmlWriter);
            }
            _currentMemberPathStack.Pop();
        }

        private void WriteArrayItems(string elemName, object arr, XmlWriter xmlWriter)
        {
            foreach (object item in (IEnumerable)arr)
                WriteObjectIfNotNull(elemName ?? item.GetType().Name, item, xmlWriter);
        }

        private void WriteObjectIfNotNull(string elemName, object obj, XmlWriter xmlWriter)
        {
            if (obj is not null)
            {
                xmlWriter.WriteStartElement(elemName);
                if (IsCustomXmlObject(obj))
                    WriteObjectMembers(obj, xmlWriter);
                else if (obj is not string || obj is string str && str != "")
                    WriteValue(Format(obj), xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }

        private void WriteValue(string value, XmlWriter xmlWriter)
        {
            string sanitizedValue = ModifyIfRequired(value);
            xmlWriter.WriteValue(sanitizedValue);

            string ModifyIfRequired(string input)
            {
                if (TryGetCurrentMemberSettings(out ClassMemberSettings settings))
                    return StrategyExecutor.Execute(input, settings.Strategy);
                else
                    return input;
            }

            bool TryGetCurrentMemberSettings(out ClassMemberSettings settings)
            {
                settings = null;
                if (_currentlySerializedObjectSettings is not null)
                {
                    string currentMemberPath = GetCurrentMemberPath();
                    if (_currentlySerializedObjectSettings.Settings.ContainsKey(currentMemberPath))
                    {
                        settings = _currentlySerializedObjectSettings.Settings[currentMemberPath];
                        return true;
                    }
                }
                return false;
            }

            string GetCurrentMemberPath()
            {
                return string.Join(".", _currentMemberPathStack.Reverse());
            }
        }

        private static string Format(object obj)
        {
            if (obj is SoapCustomTypes.Date date)
                return date.ToString();
			else if (obj is Enum)
                return GetEnumValue(obj);
            else if (obj is DateTime dateTime)
                return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
            else if (obj is bool bl)
                return bl.ToString().ToLower();
            else if (obj is decimal dc)
                return dc.ToString(CultureInfo.InvariantCulture);
            else if (obj is double db)
                return db.ToString(CultureInfo.InvariantCulture);
            else
                return obj.ToString();
        }

        private string GetEnumValue(object obj)
        {
            var objType = obj.GetType();
            var valueName = Enum.GetName(objType, obj);
            var enumItemMember = objType.GetMember(valueName).FirstOrDefault();
            if (enumItemMember is not null)
            {
                var enumAttr = enumItemMember.GetCustomAttribute<XmlEnumAttribute>();
                if (enumAttr is not null)
                    return enumAttr.Name;
            }
            return obj.ToString();
        }

        private static bool IsCustomXmlObject(object obj)
        {
            return obj.GetType().GetCustomAttributes(typeof(XmlTypeAttribute)).Any()
                || obj.GetType().GetCustomAttributes(typeof(XmlRootAttribute)).Any()
                || obj.GetType().GetCustomAttributes(typeof(DataContractAttribute)).Any();
        }

        private static bool IsXmlMember(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(typeof(XmlElementAttribute)).Any()
                || memberInfo.GetCustomAttributes(typeof(XmlArrayAttribute)).Any()
                || memberInfo.GetCustomAttributes(typeof(XmlArrayItemAttribute)).Any()
                || memberInfo.GetCustomAttributes(typeof(MessageBodyMemberAttribute)).Any()
                || memberInfo.GetCustomAttributes(typeof(DataMemberAttribute)).Any();
        }

        private static bool IsXmlArray(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(typeof(XmlArrayAttribute)).Any()
                || memberInfo.GetCustomAttributes(typeof(XmlArrayItemAttribute)).Any()
                || typeof(IEnumerable).IsAssignableFrom(memberInfo.GetType()) && memberInfo.GetType() != typeof(string);
        }

        private static string GetElemNameFromObjectType(object obj)
        {
            if (obj.GetType().GetCustomAttributes(typeof(XmlRootAttribute)).FirstOrDefault() is XmlRootAttribute attr)
                return !string.IsNullOrEmpty(attr.ElementName) ? attr.ElementName : obj.GetType().Name;
            else
                return obj.GetType().Name;
        }

        private static string GetElemNameFromMemberInfo(MemberInfo memberInfo)
        {
            if (memberInfo.GetCustomAttributes(typeof(XmlElementAttribute)).FirstOrDefault() is XmlElementAttribute attr)
                return !string.IsNullOrEmpty(attr.ElementName) ? attr.ElementName : memberInfo.Name;
            else if (memberInfo.GetCustomAttributes(typeof(XmlArrayAttribute)).FirstOrDefault() is XmlArrayAttribute attrArr)
                return !string.IsNullOrEmpty(attrArr.ElementName) ? attrArr.ElementName : memberInfo.Name;
            else
                return memberInfo.Name;
        }

        private static string GetArrayItemNameFromMemberInfo(MemberInfo memberInfo)
        {
            if (memberInfo.GetCustomAttributes(typeof(XmlArrayItemAttribute)).FirstOrDefault() is XmlArrayItemAttribute attr)
                return !string.IsNullOrEmpty(attr.ElementName) ? attr.ElementName : memberInfo.Name;
            else
                return null;
        }
    }
}
