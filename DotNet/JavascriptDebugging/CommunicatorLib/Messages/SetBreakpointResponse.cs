using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class SetBreakpointResponse
    {
        [JsonProperty("breakpointId")]
        public string BreakpointId { get; set; }

        [JsonProperty("actualLocation")]
        public Location ActualLocation { get; set; }
    }
}