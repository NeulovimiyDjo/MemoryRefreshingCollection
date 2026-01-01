using System;
using System.Collections.Generic;

namespace XmlSerializerTests
{
    public static class XmlSerializerTestsData
    {
        public static Dictionary<string, object> CaseSamples = new()
        {
            {
                $"{nameof(TestRootType)}-filled",
                new TestRootType()
                {
                    IntMember = -17,
                    BoolMember = true,
                    DecimalMember = 13.4586m,
                    DateTimeMember = DateTime.Parse("2023-10-13T14:58:54.545497+00:00").ToUniversalTime(),
                    DateMember = new Date(DateTime.Parse("2023-10-13")),
                    StringMember = "Some Text Value",
                    EnumMember = TestEnumType.Item02,
                    ClassMember = new TestElemType() { StringMember = "Some ClassMemberX Text Value" },
                    ClassListElemMember = new TestElemType[] {
                        new TestElemType() { StringMember = "Some ClassListElemMemberX Text Value 1" },
                        new TestElemType() { StringMember = "Some ClassListElemMemberX Text Value 2" },
                    },
                    ClassListArrayMember = new List<TestElemType>() {
                        new TestElemType() { StringMember = "Some ClassListArrayMemberX Text Value 1" },
                        new TestElemType() { StringMember = "Some ClassListArrayMemberX Text Value 2" },
                    },
                    ClassListNamedItemsMember = new TestElemType[] {
                        new TestElemType() { StringMember = "Some ClassListNamedItemsMemberX Text Value 1" },
                        new TestElemType() { StringMember = "Some ClassListNamedItemsMemberX Text Value 2" },
                    },
                    StringListElemMember = new string[] { "Some StringListElemMemberX Text Value" },
                    StringListArrayMember = new List<string>() { "Some StringListArrayMemberX Text Value" },
                    StringListNamedItemsMember = new string[] { "Some StringListNamedItemsMemberX Text Value" },
                    StringListNamedItemsNoArrayMember = new string[] { "Some StringListNamedItemsNoArrayMember Text Value" },
                    DataMemberStr = "Some DataMemberStr Text Value"
                }
            },
            {
                $"{nameof(TestRootType)}-emptynodes",
                new TestRootType()
                {
                    StringMember = "",
                    ClassMember = new TestElemType(),
                    ClassListElemMember = new TestElemType[0],
                    ClassListArrayMember = new List<TestElemType>(),
                    ClassListNamedItemsMember = new TestElemType[0],
                    StringListElemMember = new string[0],
                    StringListArrayMember = new List<string>(),
                    StringListNamedItemsMember = new string[0],
                }
            },
            {
                $"{nameof(TestRootType)}-nullnodes",
                new TestRootType()
            },
        };
    }
}
