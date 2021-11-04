using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class EvaluateOnCallFrameResponse
    {
        [JsonProperty("result")]
        public RemoteObject Result { get; set; }
    }
}