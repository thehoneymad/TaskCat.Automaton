namespace TaskCat.Automaton
{
    using System.Collections.Generic;

    public class MachineSave
    {
        public string Name { get; private set; }
        public string Variant { get; private set; }

        public bool IsResolved { get; private set; }
        public bool IsInitialized { get; private set; }

        public IEnumerable<Node> CurrentCandidateNodes { get; set; }

        public Dictionary<string, List<string>> NodeHistory { get; private set; }
        public Dictionary<string, Node> NodeDictionary { get; private set; }

        private MachineSave()
        {

        }

        public static MachineSave SaveMachine(FiniteStateMachine machine)
        {
            return new MachineSave()
            {
                Name = machine.Name,
                CurrentCandidateNodes = machine.CurrentCandidateNodes,
                IsInitialized = machine.IsInitialized,
                IsResolved = machine.IsResolved,
                NodeDictionary = machine.NodeDictionary,
                NodeHistory = machine.NodeHistory,
                Variant = machine.Variant
            };
        }
    }
}
