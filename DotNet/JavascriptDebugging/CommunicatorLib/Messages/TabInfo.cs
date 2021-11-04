using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class TabInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("webSocketDebuggerUrl")]
        public string WebSocketDebuggerUrl { get; set; }
    }
}