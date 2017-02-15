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
            this.NodeHistory.Add(currentCandiateNode);
            // This will definitely be a List since we can have multiple candidate node
        }


        public void ExecuteEvent(EventDefinition eventDef)
        {
            if (eventDef == null)
                throw new ArgumentNullException(nameof(eventDef));

            if (string.IsNullOrWhiteSpace(eventDef.FromType))
                throw new ArgumentException(nameof(eventDef.FromType));

            if (this.currentCandiateNode == null)
                throw new NullReferenceException(nameof(currentCandiateNode));

            if (this.currentCandiateNode.Id != eventDef.FromType)
                return;

            // Find a event that matches the currentCandidate
            var eventMatch = this.Events.Where(x => x.From == eventDef.FromType && IsSameOperation(x.MatchCondition, eventDef.Operation));

            if (eventMatch.Any())
            {
                // Mathing event can be more than one of course, once again, this is where we send back a choice
                // For the sake of simplicity of the first example, taking the current one.
                var selectedEvent = eventMatch.First();
                var node = this.currentCandiateNode;

                selectedEvent.Action?.ApplyTo(currentCandiateNode.Payload);

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

                    selectedEvent.Action?.ApplyTo(this.currentCandiateNode.Payload);
                    this.NodeHistory.Add(this.currentCandiateNode);
                }
                else if (selectedEvent.IsResolveEvent)
                {
                    // This means this is where the shit stops. Target should be null here. 
                    // We can check for that nullity,  of course we should do it in validation
                    selectedEvent.Action?.ApplyTo(currentCandiateNode.Payload);
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
