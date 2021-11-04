using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class GetScriptSourceResponse
    {
        [JsonProperty("scriptSource")]
        public string ScriptSource { get; set; }
    }
}