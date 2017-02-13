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

            Operation op = new Operation() {
                op = "replace",
                path = "/state",
                value = "COMPLETED"
            };

            machine.ExecuteEvent(new EventDefinition() {
                FromType = "FetchDeliveryMan",
                Operation = op
            });

            Assert.That(machine.NodeHistory.Count == 2);
            Assert.That(machine.NodeHistory.First().Type == "FetchDeliveryMan");
            Assert.That(machine.NodeHistory.Last().Type == "Pickup");
        }
    }
}
