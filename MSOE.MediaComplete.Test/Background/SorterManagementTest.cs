using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;

namespace MSOE.MediaComplete.Test.Background
{
    /// <summary>
    /// Tests for Sorter. Note that these tests focus on the behavior of 
    /// ResolveConflicts, since the "Do" method is already largely tested by SortingTest
    /// </summary>
    [TestClass]
    public class SorterManagementTest
    {
        /// <summary>
        /// Test that an empty queue correctly accepts a SortingTask
        /// </summary>
        [TestMethod]
        public void Test_AddSorterEmptyQueue_Added()
        {
            
            var queue = new List<List<Task>>();
            var subject = new Sorter(new Mock<IFileManager>().Object, null);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(1, queue.Count, "Queue doesn't have the right number of stages!");
            Assert.AreEqual(1, queue[0].Count, "Stage 1 doesn't have the new task!");
            Assert.AreSame(subject, queue[0][0], "Stage 1 doesn't have the subject task!");
        }

        /// <summary>
        /// Test that a sorter will correctly insert itself after existing Imports and Identifies
        /// </summary>
        [TestMethod]
        public void Test_AddSorter_GoesAfterImportAndIdentify()
        {
            var mock = new Mock<IFileManager>();
            var list = new List<SongPath> {new SongPath("string")};
            var queue = new List<List<Task>>
            {
                new List<Task> {new Importer(mock.Object, list, false)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Identifier(new LocalSong[]{}, null, null, null, null)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Importer(new Mock<IFileManager>().Object, list, false)},
                new List<Task>()
            };

            var subject = new Sorter(null, null);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(4, queue.Count, "Queue doesn't have the right number of stages!");
            Assert.AreEqual(1, queue[0].Count, "Stage 1 isn't the same size!");
            Assert.IsInstanceOfType(queue[0][0], typeof(Importer), "Stage 1 doesn't have an ImportTask!");
            Assert.AreEqual(2, queue[1].Count, "Stage 2 isn't the same size!");
            Assert.IsInstanceOfType(queue[1][0], typeof(Identifier), "Stage 2 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[1][1], typeof(Identifier), "Stage 2 task 2 isn't an IdentifierTask!");
            Assert.AreEqual(2, queue[2].Count, "Stage 3 isn't the same size!");
            Assert.IsInstanceOfType(queue[2][0], typeof(Identifier), "Stage 3 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[2][1], typeof(Importer), "Stage 3 task 2 isn't an ImportTask!");
            Assert.AreEqual(1, queue[3].Count, "Stage 4 doesn't have the new task!");
            Assert.AreSame(subject, queue[3][0], "Stage 4 doesn't have the subject task!");
        }

        /// <summary>
        /// Test that a sorter will remove other sorts
        /// </summary>
        [TestMethod]
        public void Test_AddSorter_RemovesSortAndGoesLast()
        {
            var mock = new Mock<IFileManager>();
            var list = new List<SongPath> { new SongPath("string") };
            var queue = new List<List<Task>>
            {
                new List<Task> {new Importer(mock.Object, list, false)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Identifier(new LocalSong[]{}, null, null, null, null)},
                new List<Task> {new Sorter(null, null)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Importer(new Mock<IFileManager>().Object, list, false)}
            };

            TaskAdder.ResolveConflicts(new Sorter(null,null), queue);

            Assert.AreEqual(4, queue.Count, "Queue doesn't have the right number of stages!");
            Assert.AreEqual(1, queue[0].Count, "Stage 1 isn't the same size!");
            Assert.IsInstanceOfType(queue[0][0], typeof(Importer), "Stage 1 doesn't have an ImportTask!");
            Assert.AreEqual(2, queue[1].Count, "Stage 2 isn't the same size!");
            Assert.IsInstanceOfType(queue[1][0], typeof(Identifier), "Stage 2 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[1][1], typeof(Identifier), "Stage 2 task 2 isn't an IdentifierTask!");
            Assert.AreEqual(2, queue[2].Count, "Stage 3 isn't the same size!");
            Assert.IsInstanceOfType(queue[2][0], typeof(Identifier), "Stage 3 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOfType(queue[2][1], typeof(Importer), "Stage 3 task 2 isn't an ImportTask!");
            Assert.AreEqual(1, queue[3].Count, "Stage 4 doesn't have the new task!");
        }
    }
}
