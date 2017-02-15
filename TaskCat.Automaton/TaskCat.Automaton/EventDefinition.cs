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
        /// Event/Message to be invoked from type
        /// </summary>
        public string FromType { get; set; }

        /// <summary>
        /// JSONPatch operation to act on the node
        /// </summary>
        public Operation Operation { get; set; }
    }
}
