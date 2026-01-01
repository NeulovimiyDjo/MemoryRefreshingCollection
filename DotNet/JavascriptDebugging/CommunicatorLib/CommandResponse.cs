using CommunicatorLib.Messages;

namespace CommunicatorLib
{
    public class CommandResponse : IV8EventParameters
    {
        public int RequestId { get; }
        public string RawJson { get; }

        public CommandResponse(int requestId, string rawJson)
        {
            RequestId = requestId;
            RawJson = rawJson;
        }
    }
}