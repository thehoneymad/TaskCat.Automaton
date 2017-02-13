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
        private Node currentCandiateNode;

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
        /// Boolean trigger to let the Finite State Machine know
        /// that this is a forest instead of a single state tree
        /// </summary>
        [JsonProperty("isForest")]
        public bool IsForest { get; set; }

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

            if (!this.IsForest && this.Nodes?.Count(x => x.IsEntryNode) > 1)
            {
                throw new NotSupportedException("Finite state machine is not expected to have multiple start states yet it has more than 1 start state");
            }

            // TODO: Find out more and more validation points with time. 
        }

        private void Initiate()
        {
            // INFO: This is ghetto. This definitely can be a NFA where I can have multiple
            // start states. For the prelimiary code, Im taking the first start node to drive
            // to a result.

            var startNode = this.Nodes.First(x => x.IsEntryNode);
            this.currentCandiateNode = startNode;
            // This will definitely be a List since we can have multiple candidate node
        }


        private dynamic ExecuteEvent(EventDefinition eventDef)
        {
            if (eventDef == null)
                throw new ArgumentNullException(nameof(eventDef));

            if (string.IsNullOrWhiteSpace(eventDef.FromType))
                throw new ArgumentException(nameof(eventDef.FromType));

            if (this.currentCandiateNode == null)
                throw new NullReferenceException(nameof(currentCandiateNode));

            // Find a event that matches the currentCandidate
            var eventMatch = this.Events.Where(x => x.From == eventDef.FromType && IsSameOperation(x.MatchCondition, eventDef.Operation));
            if (eventMatch.Any())
            {
                // Mathing event can be more than one of course, once again, this is where we send back a choice
                // For the sake of simplicity of the first example, taking the first one.
                var selectedEvent = eventMatch.First();
                var node = this.Nodes.First(x => x.Type == eventDef.FromType);
                selectedEvent.Action.ApplyTo(node.Payload);
                return node.Payload;               
            }
            else
            {
                return null; // TODO: Need to devise a nice looking exception for this.
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
