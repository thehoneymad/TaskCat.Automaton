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


        public List<Node> NodeHistory { get; private set; } = new List<Node>();

        public bool IsResolved { get; private set; }

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
            fsm.Initiate();
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
                fsm.Initiate();
                return fsm;
            }
        }

        private void Validate()
        {
            if (Nodes.GroupBy(x => x.Id).Count() != this.Nodes.Count)
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

            // TODO: Find out more and more validation points with time. 
        }

        private void Initiate(string startNodeType = null)
        {
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

            // Add current starting node as a candidate node to take transition
            this.currentCandidateNodes.Add(startingNode);
        }

        public void ExecuteEvent(EventDefinition eventDef)
        {
            if (eventDef == null)
                throw new ArgumentNullException(nameof(eventDef));

            if (string.IsNullOrWhiteSpace(eventDef.Type))
                throw new ArgumentException(nameof(eventDef.Type));

            if (this.currentCandidateNodes == null || !currentCandidateNodes.Any())
                throw new NotSupportedException($"{nameof(currentCandidateNodes)} is either empty or null");

            var currentCandiateNode = this.currentCandidateNodes.FirstOrDefault(x => x.Type == eventDef.Type);
            if (currentCandiateNode == null)
                return;

            // Find a event that matches the currentCandidate
            var eventMatch = this.Events.Where(x => x.From == eventDef.Type
                && IsSameOperation(x.MatchCondition, eventDef.Operation));

            if (eventMatch.Any())
            {
                // Mathing event can be more than one of course, once again, this is where we send back a choice
                // For the sake of simplicity of the first example, taking the current one.
                var selectedEvent = eventMatch.First();
                var node = this.currentCandiateNode;


                // We got the node. Lets create the next node. 
                // Do we need to create a duplicate 
                if (selectedEvent.From == selectedEvent.Target && selectedEvent.CreateNewTarget)
                {

                    // clone the current node. This is JS style cloing, need to write a deep cloning method
                    var nodestring = JsonConvert.SerializeObject(node);
                    var cloneNode = JsonConvert.DeserializeObject<Node>(nodestring);

                    // We have to inject a RESET method for this nodes. We dont have that.
                    // So we have to work with what we have now, that means nothing, we have nothing now. just clone this shit
                    // And forget. We can check that later.

                    this.currentCandiateNode = cloneNode;

                    this.NodeHistory.Add(this.currentCandiateNode);
                }
                else if (selectedEvent.IsResolveEvent)
                {
                    // This means this is where the shit stops. Target should be null here. 
                    // We can check for that nullity,  of course we should do it in validation
                    this.IsResolved = true;
                }
                else
                {
                    // Need to add the target node here. We don't yet know how to get a sample
                    // or generator for that. We might need to think about that here.

                    // For now, lets just make sure it is testable. 
                    var dummyNode = this.Nodes.First(x => x.Id == selectedEvent.Target);
                    this.currentCandiateNode = dummyNode;
                    this.NodeHistory.Add(dummyNode);
                }

                // Change application is done.               
            }
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
