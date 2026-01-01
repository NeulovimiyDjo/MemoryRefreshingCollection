using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class GetPropertiesResponse
    {
        [JsonProperty("result")]
        public PropertyDescriptor[] PropertyDescriptors { get; set; }
    }
}