using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace WCFServices
{
    [DataContract(Name = "Test_TestServiceRequest3", Namespace = Constants.XmlNamespace)]
    public class Test_TestServiceRequest3
    {
        [DataMember(Name = "field1", IsRequired = true, Order = 0)]
        public string field1;

        [DataMember(Name = "field2", IsRequired = true, Order = 1)]
        [JsonProperty(PropertyName = "fieldTest2")]
        public string field2_testtest;

        [DataMember(Name = "arr3", IsRequired = true, Order = 2)]
        public string[] arr3;
    }
}
