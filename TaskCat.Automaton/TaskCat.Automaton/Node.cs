namespace TaskCat.Automaton
{
    using Newtonsoft.Json;

    public class Node
    {
        /// <summary>
        /// Unique identifier for the node
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Node type, this matches against the default set of node types
        /// for the state machine
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Defines whether the node is an entry node or not
        /// </summary>
        [JsonProperty("isEntryNode")]
        public bool IsEntryNode { get; set; }


        [JsonProperty("isResolveNode")]
        public bool IsResolveNode { get; set; }

        [JsonProperty("payload")]
        public dynamic Payload { get; set; }
    }
}
