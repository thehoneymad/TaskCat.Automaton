using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskCat.Automaton.Tests
{
    [TestFixture]
    public class TestFiniteStateMachine
    {
        [Test]
        public void TestCreation()
        {
            FiniteStateMachine machine = new FiniteStateMachine();
            Assert.IsNotNull(machine);
        }
    }
}
