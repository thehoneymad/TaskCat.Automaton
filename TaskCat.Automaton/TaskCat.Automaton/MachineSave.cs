namespace TaskCat.Automaton
{
    using System.Collections.Generic;

    public class MachineSave
    {
        public Dictionary<string, List<string>> NodeHistory { get; private set; }
        public Dictionary<string, Node> NodeDictionary { get; private set; }

        public MachineSave(
            Dictionary<string, List<string>> nodeHistory,
            Dictionary<string, Node> nodeDictionary)
        {
            this.NodeDictionary = nodeDictionary;
            this.NodeHistory = nodeHistory;
        }
    }
}
