The sample state machine diagram looks like this. If `createNewTarget` is set to `true` then the node will create a new node in the chain rather than coming back to itself

<img src='http://g.gravizo.com/g?
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
'/>