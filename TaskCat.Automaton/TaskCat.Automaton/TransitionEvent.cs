using Marvin.JsonPatch.Operations;
using Newtonsoft.Json;

namespace TaskCat.Automaton
{
    public class TransitionEvent
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("from", Required = Required.Always)]
        public string From { get; set; }

        [JsonProperty("target", Required = Required.Always)]
        public string Target { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("matchCondition", Required = Required.Always)]
        public Operation MatchCondition { get; set; }

        [JsonProperty("createNewTarget")]
        public bool CreateNewTarget { get; set; }

        [JsonProperty("maxEventRetry")]
        public int MaxEventRetry { get; set; }

        [JsonProperty("isResolveEvent")]
        public bool IsResolveEvent { get; set; }
    }
}
