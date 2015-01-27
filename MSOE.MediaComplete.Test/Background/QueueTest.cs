using System;
using System.Collections.Generic;
using System.Threading;
using Sys = System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Background;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MSOE.MediaComplete.Test.Background
{
    [TestClass]
    public class QueueTest
    {
        private const int Timeout = 30000;
        private const int Delay = 20000;

        [TestMethod, Timeout(Timeout)]
        public void Add_Task_CallsResolveConflicts()
        {
            var mock = new MockTask();
            Queue.Inst.Add(mock);

            SpinWait.SpinUntil(() => mock.DoCalled);
        }

        [TestMethod, Timeout(Timeout)]
        public void Add_QueueAlreadyRunning_NewTaskRuns()
        {
            var longMock = new MockTask(Delay);
            Queue.Inst.Add(longMock);

            var secondMock = new MockTask();
            Queue.Inst.Add(secondMock);

            Assert.IsFalse(secondMock.DoCalled, "Second mock started too early!");

            // Wait for the second task to run
            SpinWait.SpinUntil(() => secondMock.DoCalled);
        }

        private class MockTask : Task
        {
            public bool DoCalled { get; private set; }
            private readonly int _doDelay;

            public MockTask(int doDelay = 0)
            {
                DoCalled = false;
                _doDelay = doDelay;
            }

            public override void Do(int i)
            {
                DoCalled = true;

                Thread.SpinWait(_doDelay);
            }

            public override IReadOnlyCollection<Type> InvalidBeforeTypes
            {
                get { return new List<Type>().AsReadOnly(); }
            }

            public override IReadOnlyCollection<Type> InvalidAfterTypes
            {
                get { return new List<Type>().AsReadOnly(); }
            }

            public override IReadOnlyCollection<Type> InvalidDuringTypes
            {
                get { return new List<Type>().AsReadOnly(); }
            }

            public override bool RemoveOther(Task t)
            {
                return false;
            }
        }
    }
}
