using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class DebuggerPausedEvent : IV8EventParameters
    {
        [JsonProperty("callFrames")]
        public CallFrame[] CallFrames { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}