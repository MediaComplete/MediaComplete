using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;
using System.Collections.Generic;
using Moq;
using MSOE.MediaComplete.Lib.Files;

namespace MSOE.MediaComplete.Test.Background
{
    /// <summary>
    /// Tests for ImportTask. Note that these tests focus on the behavior of 
    /// ResolveConflicts, since the "Do" method is already largely tested by ImporterTest
    /// </summary>
    [TestClass]
    public class ImporterManagementTest
    {
        /// <summary>
        /// Test that an empty queue correctly accepts an ImportTask
        /// </summary>
        [TestMethod]
        public void Test_AddImportEmptyQueue_Added()
        {
            var mock = new Mock<IFileManager>();
            var queue = new List<List<Task>>();
            var subject = new Importer(mock.Object, new List<SongPath> { new SongPath("") }, false);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(1, queue.Count, "Queue doesn't have the right number of stages!");
            Assert.AreEqual(1, queue[0].Count, "Stage 1 doesn't have the new task!");
            Assert.AreSame(subject, queue[0][0], "Stage 1 doesn't have the subject task!");
        }

        /// <summary>
        /// Test that an import will correctly insert itself after existing Sorts and Identifies
        /// </summary>
        [TestMethod]
        public void Test_AddImport_GoesBeforeSortAndIdentify()
        {
            var mock = new Mock<IFileManager>();
            var queue = new List<List<Task>>
            {
                new List<Task> {new Sorter(mock.Object, null)},
                new List<Task> {new IdentifierTask(), new IdentifierTask()},
                new List<Task> {new IdentifierTask(), new Sorter(mock.Object, null)},
                new List<Task>()
            };

            var subject = new Importer(mock.Object, new List<SongPath> { new SongPath("") }, false);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(4, queue.Count, "Queue doesn't have the right number of stages!");

            Assert.AreEqual(1, queue[0].Count, "Stage 1 doesn't have the new task!");
            Assert.AreSame(subject, queue[0][0], "Stage 1 doesn't have the subject task!");
            Assert.AreEqual(1, queue[1].Count, "Stage 2 isn't the same size!");
            Assert.IsInstanceOfType(queue[1][0], typeof(Sorter), "Stage 2 doesn't have an SortingTask!");
            Assert.AreEqual(2, queue[2].Count, "Stage 3 isn't the same size!");
            Assert.IsInstanceOfType(queue[2][0], typeof(IdentifierTask), "Stage 3 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[2][1], typeof(IdentifierTask), "Stage 3 task 2 isn't an IdentifierTask!");
            Assert.AreEqual(2, queue[3].Count, "Stage 4 isn't the same size!");
            Assert.IsInstanceOfType(queue[3][0], typeof(IdentifierTask), "Stage 4 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[3][1], typeof(Sorter), "Stage 4 task 2 isn't an ImportTask!");

        }

        /// <summary>
        /// Test that an import will run in parallel with other imports
        /// </summary>
        [TestMethod]
        public void Test_AddImport_GoesWithOtherImport()
        {
            var mock = new Mock<IFileManager>();
            var queue = new List<List<Task>>
            {
                new List<Task> {new Importer(mock.Object, new List<SongPath>{new SongPath("")}, false)},
                new List<Task> {new IdentifierTask(), new IdentifierTask()}
            };

            var subject = new Importer(mock.Object, new List<SongPath> { new SongPath("") }, false);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(2, queue.Count, "Queue doesn't have the right number of stages!");

            Assert.AreEqual(2, queue[0].Count, "Stage 1 doesn't have the new task!");
            Assert.IsInstanceOfType(queue[0][0], typeof(Importer), "Stage 1 doesn't have the old import task!");
            Assert.AreSame(subject, queue[0][1], "Stage 1 doesn't have the subject task!");
            Assert.AreEqual(2, queue[1].Count, "Stage 2 isn't the same size!");
            Assert.IsInstanceOfType(queue[1][0], typeof(IdentifierTask), "Stage 2 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[1][1], typeof(IdentifierTask), "Stage 2 task 2 isn't an IdentifierTask!");

        }
    }
}