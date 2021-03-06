using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class V8DebuggerEvent<T> where T: new()
    {
        [JsonProperty("method")]
        public string MethodName { get; set; }

        [JsonProperty("params")]
        public T EventParameters { get; set; }
    }
}