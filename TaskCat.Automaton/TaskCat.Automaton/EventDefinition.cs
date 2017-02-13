using Marvin.JsonPatch.Operations;

namespace TaskCat.Automaton
{
    public class EventDefinition
    {
        public string FromType { get; set; }
        public Operation Operation { get; set; }
    }
}
