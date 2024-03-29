﻿using Newtonsoft.Json;

namespace CommunicatorLib.Messages
{
    public class PropertyDescriptor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public RemoteObject Value { get; set; }

        // ... Much more
    }
}