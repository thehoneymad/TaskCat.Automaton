# TaskCat.Automataton
Json driven finite state machine.

Essentially the need of this could've been simplified using a simple finite state machine. Since this was a part of TaskCat, we figured we need something tailor made for us. A lot of systems are using JSON not only as the primary data interchange format but also the primary storage format now a days. Thanks goes the the document driven databases like MongoDb. Handling a system that essentially works with workflows gets tougher and tougher since the real life workflows are not easily predictable, sometimes not even understood up until a certain time has invested into development.

TaskCat.Automaton is a another finite state machine but the transition conditions for this state machines are defined by JSONPatch defined in RFC-6902. The reason behind this was to trigger a document state from one state to another using JSONPatch on the document. Although TaskCat.Automaton doesn't modify the actual document but still it is indeed helpful when you can just ask a state machine to actuate upon any condition you can define through a JSONPatch on any JSON document.

The sample state machine diagram used in the unit tests looks like this. If `createNewTarget` property of the machine is set to `true` then the node will create a new node in the chain rather than coming back to itself. TaskCat.Automaton currently clones the definition to create a new node as this is generative. So, your system doesn't have to have all the nodes to begin with. Our current plan is to make sure that it only takes document driven by a JsonSchema and generates default documents (nodes) from that schema so no system has to know all the nodes right in front and you only save what you have traversed.

![Sample FSM](https://g.gravizo.com/g?
 digraph G {
   pickup -> pickup [style=bold, label="FAILED (change variant to retry)"];
   pickup -> delivery [label="COMPLETED"];
   delivery -> delivery [style=bold, label="FAILED (change variant to retry)"];
   delivery -> ReturnToSeller [label="RETURNED"];
   ReturnToSeller -> ReturnToSeller [style=bold, label="FAILED (change variant to retry)"];
   ReturnToSeller -> ReturnToWarehouse [style=bold, label="FAILED"];
   ReturnToWarehouse -> ReturnToSeller [label="COMPLETED"];
   ReturnToWarehouse -> ReturnToWarehouse [style=bold, label="FAILED (change variant to retry)"];
   ReturnToWarehouse -> resolved [label="COMPLETED"];
   delivery -> ReturnToWarehouse [label="RETURNED"];
   resolved [shape=box,style=filled,color=".7 .3 1.0"];
   delivery -> resolved [label="COMPLETED"];
   ReturnToSeller -> resolved [label="COMPLETED"];
 }
)

The simple state machine based on this graph can be found over https://github.com/thehoneymad/TaskCat.Automaton/blob/master/TaskCat.Automaton/TaskCat.Automaton.Tests/SampleFSM.md
