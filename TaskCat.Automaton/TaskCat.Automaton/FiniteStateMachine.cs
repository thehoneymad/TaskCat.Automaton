namespace TaskCat.Automaton
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Marvin.JsonPatch.Operations;

    public class FiniteStateMachine
    {
        private HashSet<Node> currentCandidateNodes = new HashSet<Node>();
        public IEnumerable<Node> CurrentCandidateNodes { get { return currentCandidateNodes; } }

        /// <summary>
        /// Name of the finite state machine.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Possible variant of the finite state machine.
        /// This is for having different variants of a single finite machine
        /// </summary>
        [JsonProperty("variant")]
        public string Variant { get; set; } = "default";

        /// <summary>
        /// Nodes allowed in the finite state machine
        /// </summary>
        [JsonProperty("nodes")]
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// Events/Transition definitions for the machine
        /// </summary>
        [JsonProperty("events")]
        public List<TransitionEvent> Events { get; set; }

        // TODO: May be a separate class to handle these?
        public Dictionary<string, List<string>> NodeHistory { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, Node> NodeDictionary { get; set; } = new Dictionary<string, Node>();

        public bool IsResolved { get; private set; }
        public bool IsInitialized { get; private set; }

        private FiniteStateMachine()
        {

        }

        /// <summary>
        /// Load a finite state machine from a json string.
        /// </summary>
        /// <param name="json">json document of a state machine</param>
        /// <returns></returns>
        public static FiniteStateMachine FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException(nameof(json));

            var fsm = JsonConvert.DeserializeObject<FiniteStateMachine>(json);
            if (fsm == null)
                throw new NullReferenceException(nameof(fsm));

            fsm.Validate();
            return fsm;
        }

        /// <summary>
        /// Load a finite state machine from file path.
        /// </summary>
        /// <param name="filePath">File path where the json state machine is located.</param>
        /// <returns></returns>
        public static FiniteStateMachine FromFile(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                var fsm = serializer.Deserialize(file, typeof(FiniteStateMachine)) as FiniteStateMachine;
                if (fsm == null)
                    throw new NullReferenceException(nameof(fsm));

                fsm.Validate();
                return fsm;
            }
        }

        private void Validate()
        {
            if (Nodes.GroupBy(x => x.Type).Count() != this.Nodes.Count)
            {
                throw new NotSupportedException("Node types should be unique, duplicate node detected");
            }

            if (!this.Nodes.Any(x => x.IsEntryNode))
            {
                throw new NotSupportedException("Finite state machine has no entry node");
            }

            if (!this.Nodes.Any(x => x.IsResolveNode))
            {
                throw new NotSupportedException("Finite state machine has no resolve/end node");
            }

            if (this.Events.GroupBy(x => x.Id).Count() != this.Events.Count)
            {
                throw new NotSupportedException("Event ids should be unique, duplicate event id detected");
            }

            if (this.Events.Any(x => x.IsResolveEvent && x.Target != null))
            {
                throw new NotSupportedException("Resolvable events should not have a target");
            }

            // TODO: Find out more and more validation points with time. 
        }

        public void Initiate(string startNodeType = null)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Machine seems to have been initialized already");

            Node startingNode = null;
            if (string.IsNullOrWhiteSpace(startNodeType))
            {
                startingNode = this.Nodes.First(x => x.IsEntryNode);
            }
            else
            {
                var selectedNode = this.Nodes.FirstOrDefault(x => x.IsEntryNode && x.Type == startNodeType);
                if (selectedNode == null)
                    throw new ArgumentException($"{startNodeType} is not a valid starting node type");
            }

            startingNode.Id = Guid.NewGuid().ToString();
            // Add current starting node as a candidate node to take transition
            this.currentCandidateNodes.Add(startingNode);

            this.IsInitialized = true;
        }

        public IEnumerable<TransitionEvent> GetEvents(string candidateNodeId)
        {
            var candidateNode = this.currentCandidateNodes.FirstOrDefault(x => x.Id == candidateNodeId);
            if (candidateNodeId == null)
                throw new ArgumentException($"Couldn't find {candidateNodeId} in current candidates");

            var events = this.Events.Where(x => x.From == candidateNode.Type);
            return events;
        }

        public void ExecuteEvent(EventDefinition eventDef)
        {
            if (eventDef == null)
                throw new ArgumentNullException(nameof(eventDef));

            if (string.IsNullOrWhiteSpace(eventDef.Id))
                throw new ArgumentException(nameof(eventDef.Id));

            if (this.currentCandidateNodes == null || !currentCandidateNodes.Any())
                throw new NotSupportedException($"{nameof(currentCandidateNodes)} is either empty or null");

            var currentCandidateNode = this.currentCandidateNodes.FirstOrDefault(x => x.Id == eventDef.NodeId);
            if (currentCandidateNode == null)
                return;

            // Removing the selected candidate node since it will go down to history anyway
            this.currentCandidateNodes.Remove(currentCandidateNode);

            // Find a event that matches the currentCandidate
            var selectedEvent = this.Events.First(
                x => x.Id == eventDef.Id
                && x.From == currentCandidateNode.Type
                && IsSameOperation(eventDef.MatchCondition, x.MatchCondition)
            );

            // TODO: This has to show the graphical state of the work here,
            // Need to update this with proper adjacency
            var previousNode = currentCandidateNode;

            if (selectedEvent.IsResolveEvent)
            {
                this.IsResolved = true;
                return;
            }
            else if (selectedEvent.From == selectedEvent.Target)
            {
                if (selectedEvent.CreateNewTarget)
                {
                    // TODO: We need to use a schema here to resemble
                    // the nodes, so we can initiate a new one.
                    // we might need some extra logic to do that too.
                    // For now just cloning and adding a new Id should be enough

                    var newNode = JsonConvert.DeserializeObject<Node>(JsonConvert.SerializeObject(currentCandidateNode));
                    newNode.Id = Guid.NewGuid().ToString();
                    currentCandidateNode = newNode;
                }
                // TODO: Increment the retry count and add it to the registry 
            }
            else
            {
                // TODO: Need to add the target node here. We don't yet know how to get a sample
                // or generator for that. We might need to think about that here.

                // For now, lets just make sure it is testable. 

                var dummyNode = this.Nodes.FirstOrDefault(x => x.Type == selectedEvent.Target);
                if (dummyNode == null)
                    throw new NullReferenceException($"Node of type {selectedEvent.Target} is not present in Nodes");

                dummyNode.Id = Guid.NewGuid().ToString();
                currentCandidateNode = dummyNode;
            }

            this.AddToHistory(previousNode, currentCandidateNode);
            this.currentCandidateNodes.Add(currentCandidateNode);
        }

        private void AddToHistory(Node previousNode, Node currentCandidateNode)
        {
            if (previousNode == currentCandidateNode)
                return;

            // Add node to dictionary first
            NodeDictionary.Add(previousNode.Id, previousNode);


            if (NodeHistory.ContainsKey(previousNode.Id)) { NodeHistory[previousNode.Id].Add(currentCandidateNode.Id); }
            else { NodeHistory[previousNode.Id] = new List<string>(); }

            NodeHistory[previousNode.Id].Add(currentCandidateNode.Id);
        }

        // TODO: Refactor the open source codebase of Marvin.JsonPatch or write a equality comparer, this is shit.
        private bool IsSameOperation(Operation op1, Operation op2)
        {
            return op1.from == op2.from
                && op1.op == op2.op
                && op1.OperationType == op2.OperationType
                && op1.path == op2.path;
        }
    }
}
