using Marvin.JsonPatch.Operations;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TaskCat.Automaton.Tests
{
    [TestFixture]
    public class TestFiniteStateMachine
    {
        public string ExecutingPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Test]
        public void TestCreation_NotNull()
        {
            var jsonFsm = File.ReadAllText($"{ExecutingPath}\\SampleFSM.json");
            Assert.IsNotNull(jsonFsm);

            var machine = FiniteStateMachine.FromJson(jsonFsm);
            Assert.IsNotNull(machine);
            Assert.AreEqual("ClassifiedDelivery", machine.Name);
            Assert.AreEqual("default", machine.Variant);
            Assert.IsNotNull(machine.Nodes);
            Assert.IsNotNull(machine.Events);
        }

        [Test]
        public void TestExecuteSampleEvent()
        {
            var jsonFsm = File.ReadAllText($"{ExecutingPath}\\SampleFSM.json");
            Assert.IsNotNull(jsonFsm);

            var machine = FiniteStateMachine.FromJson(jsonFsm);
            Assert.IsNotNull(machine);

            Operation op = new Operation()
            {
                op = "replace",
                path = "/state",
                value = "COMPLETED"
            };

            var currentCandidates = machine.CurrentCandidateNodes;

            Assert.IsNotNull(currentCandidates);
            Assert.AreEqual(1, currentCandidates.Count());

            var candidate = currentCandidates.First();

            var eventPossibilities = machine.GetEvents(candidate.Id);
            Assert.IsNotNull(eventPossibilities);
            Assert.That(eventPossibilities.Count() >= 1);

            machine.ExecuteEvent(new EventDefinition()
            {
                Id = eventPossibilities.First().Id,
                NodeId = candidate.Id,
                MatchCondition = op
            });

            Assert.That(machine.NodeHistory.Count == 1);
            Assert.IsNotNull(machine.NodeHistory.Last());
            Assert.IsNotNull(machine.NodeHistory.Last().Value.Count == 1);
            Assert.That(machine.NodeDictionary.First().Value.Type == "Pickup");
            Assert.That(machine.CurrentCandidateNodes.Count() == 1);
            Assert.That(machine.CurrentCandidateNodes.Last().Type == "Delivery");
        }
    }
}
