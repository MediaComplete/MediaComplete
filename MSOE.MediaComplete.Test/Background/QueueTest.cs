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
        [TestMethod]
        public void Add_Task_CallsResolveConflicts()
        {
            var mock = new MockTask();
            Queue.Inst.Add(mock);

            Assert.IsTrue(mock.ResolveConflictsCalled, "Mock didn't have its conflicts resolved!");
            Assert.IsTrue(mock.DoCalled, "Mock wasn't run!");
        }

        [TestMethod]
        public void Add_QueueAlreadyRunning_NewTaskRuns()
        {
            var longMock = new MockTask(0, 25000);
            Queue.Inst.Add(longMock);

            var secondMock = new MockTask();
            Queue.Inst.Add(secondMock);

            Assert.IsTrue(secondMock.ResolveConflictsCalled, "Second mock didn't have its conflicts resolved!");
            Assert.IsFalse(secondMock.DoCalled, "Second mock started too early!");

            // Wait for the second task to run
            SpinWait.SpinUntil(() => !secondMock.DoCalled, 30000);
        }

        private class MockTask : Task
        {
            public bool ResolveConflictsCalled { get; private set; }
            public bool DoCalled { get; private set; }
            private readonly int _resolveConflictsDelay;
            private readonly int _doDelay;

            public MockTask(int resolveConflictsDelay = 0, int doDelay = 0)
            {
                ResolveConflictsCalled = false;
                DoCalled = false;
                _resolveConflictsDelay = resolveConflictsDelay;
                _doDelay = doDelay;
            }

            public override void ResolveConflicts(Dictionary<int, List<Task>> currentQueue)
            {
                ResolveConflictsCalled = true;
                currentQueue[0] = new List<Task> { this };
                Thread.Sleep(_resolveConflictsDelay);
            }

            public override Sys.Task Do(int i)
            {
                DoCalled = true;

                return Sys.Task.Delay(_doDelay);
            }
        }
    }
}
