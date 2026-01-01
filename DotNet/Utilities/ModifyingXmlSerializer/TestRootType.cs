using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace XmlSerializerTests
{
    [XmlRoot(ElementName = "TestRootTypeX")]
    public class TestRootType
    {
        [XmlElement(ElementName = "IntMemberX")]
        public int? IntMember;
        [XmlIgnore]
        public bool IntMemberSpecified => IntMember is not null;

        [XmlElement(ElementName = "BoolMemberX")]
        public bool? BoolMember;
        [XmlIgnore]
        public bool BoolMemberSpecified => BoolMember is not null;

        [XmlElement(ElementName = "DecimalMemberX")]
        public decimal? DecimalMember;
        [XmlIgnore]
        public bool DecimalMemberSpecified => DecimalMember is not null;

        [XmlElement(ElementName = "DateTimeMemberX")]
        public DateTime? DateTimeMember;
        [XmlIgnore]
        public bool DateTimeMemberSpecified => DateTimeMember is not null;

        [XmlElement(ElementName = "DateMemberX")]
        public Date DateMember;

        [XmlElement(ElementName = "StringMemberX")]
        public string StringMember;

        [XmlElement(ElementName = "EnumMemberX")]
        public TestEnumType? EnumMember;

        [XmlIgnore]
        public bool EnumMemberSpecified => EnumMember is not null;

        [XmlElement(ElementName = "ClassMemberX")]
        public TestElemType ClassMember;

        [XmlElement(ElementName = "ClassListElemMemberX")]
        public TestElemType[] ClassListElemMember;

        [XmlArray(ElementName = "ClassListArrayMemberX")]
        public List<TestElemType> ClassListArrayMember;

        [XmlArray(ElementName = "ClassListNamedItemsMemberX")]
        [XmlArrayItem(ElementName = "ArrayItemX")]
        public TestElemType[] ClassListNamedItemsMember;

        [XmlElement(ElementName = "StringListElemMemberX")]
        public string[] StringListElemMember;

        [XmlArray(ElementName = "StringListArrayMemberX")]
        public List<string> StringListArrayMember;

        [XmlArray(ElementName = "StringListNamedItemsMemberX")]
        [XmlArrayItem(ElementName = "ArrayItemX")]
        public string[] StringListNamedItemsMember;

        [XmlArrayItem(ElementName = "ArrayItemX")]
        public string[] StringListNamedItemsNoArrayMember;

        [DataMember]
        public string DataMemberStr;
    }

    [XmlRoot(ElementName = "TestElemTypeX")]
    public class TestElemType
    {
        [XmlElement(ElementName = "StringMemberX")]
        public string StringMember;
    }
    
    public enum TestEnumType
    {
        [XmlEnumAttribute("01")]
        Item01,

        [XmlEnumAttribute("02")]
        Item02,
    }
}
