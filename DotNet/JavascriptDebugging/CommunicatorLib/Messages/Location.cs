using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class Location
    {
        [JsonProperty("scriptId")]
        public string ScriptId { get; set; }

        [JsonProperty("lineNumber")]
        public int LineNumber { get; set; }

        [JsonProperty("columnNumber")]
        public int ColumnNumber { get; set; }
    }
}