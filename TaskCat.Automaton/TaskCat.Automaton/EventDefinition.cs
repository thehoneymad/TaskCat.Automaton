namespace TaskCat.Automaton
{
    using Marvin.JsonPatch.Operations;

    /// <summary>
    /// JSONPatch event to be sent as a message/event to 
    /// the finite state machine
    /// </summary>
    public class EventDefinition
    {
        /// <summary>
        /// Event Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Candidate node id
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// JSONPatch operation to act on the node
        /// </summary>
        public Operation MatchCondition { get; set; }
    }
}
