using Marvin.JsonPatch.Dynamic;
using Marvin.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaskCat.Automaton
{
    public class TransitionEvent
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("matchCondition")]
        public Operation MatchCondition { get; set; }

        [JsonProperty("createNewTarget")]
        public bool CreateNewTarget { get; set; }

        [JsonProperty("maxEventRetry")]
        public int MaxEventRetry { get; set; }

        [JsonProperty("isResolveEvent")]
        public bool IsResolveEvent { get; set; }
    }
}
