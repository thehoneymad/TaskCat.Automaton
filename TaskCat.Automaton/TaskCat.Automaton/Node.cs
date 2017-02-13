using Newtonsoft.Json;

namespace TaskCat.Automaton
{
    public class Node
    { 
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isEntryNode")]
        public bool IsEntryNode { get; set; }

        [JsonProperty("isDuplicateAllowed")]
        public bool IsDuplicateAllowed { get; set; }

        [JsonProperty("isResolveNode")]
        public bool IsResolveNode { get; set; }

        [JsonProperty("payload")]
        public dynamic Payload { get; set; }
    }
}
