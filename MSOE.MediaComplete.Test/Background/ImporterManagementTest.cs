using MediaComplete.Lib.Background;
using MediaComplete.Lib.Import;
using MediaComplete.Lib.Metadata;
using MediaComplete.Lib.Sorting;
using System.Collections.Generic;
using Moq;
using MediaComplete.Lib.Library.DataSource;
using NUnit.Framework;

namespace MediaComplete.Test.Background
{
    /// <summary>
    /// Tests for ImportTask. Note that these tests focus on the behavior of 
    /// ResolveConflicts, since the "Do" method is already largely tested by ImporterTest
    /// </summary>
    [TestFixture]
    public class ImporterManagementTest
    {
        /// <summary>
        /// Test that an empty queue correctly accepts an ImportTask
        /// </summary>
        [Test]
        public void Test_AddImportEmptyQueue_Added()
        {
            var mock = new Mock<IFileSystem>();
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
        [Test]
        public void Test_AddImport_GoesBeforeSortAndIdentify()
        {
            var mockfs = new Mock<IFileSystem>();
            var mockl = new Mock<IFileSystem>();
            var queue = new List<List<Task>>
            {
                new List<Task> {new Sorter(mockl.Object, null)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Identifier(new LocalSong[]{}, null, null, null, null)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Sorter(mockl.Object, null)},
                new List<Task>()
            };

            var subject = new Importer(mockfs.Object, new List<SongPath> { new SongPath("") }, false);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(4, queue.Count, "Queue doesn't have the right number of stages!");

            Assert.AreEqual(1, queue[0].Count, "Stage 1 doesn't have the new task!");
            Assert.AreSame(subject, queue[0][0], "Stage 1 doesn't have the subject task!");
            Assert.AreEqual(1, queue[1].Count, "Stage 2 isn't the same size!");
            Assert.IsInstanceOf(typeof(Sorter), queue[1][0], "Stage 2 doesn't have an SortingTask!");
            Assert.AreEqual(2, queue[2].Count, "Stage 3 isn't the same size!");
            Assert.IsInstanceOf(typeof(Identifier), queue[2][0], "Stage 3 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOf(typeof(Identifier), queue[2][1], "Stage 3 task 2 isn't an IdentifierTask!");
            Assert.AreEqual(2, queue[3].Count, "Stage 4 isn't the same size!");
            Assert.IsInstanceOf(typeof(Identifier), queue[3][0], "Stage 4 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOf(typeof(Sorter), queue[3][1], "Stage 4 task 2 isn't an ImportTask!");

        }

        /// <summary>
        /// Test that an import will run in parallel with other imports
        /// </summary>
        [Test]
        public void Test_AddImport_GoesWithOtherImport()
        {
            var mockfs = new Mock<IFileSystem>();
            var queue = new List<List<Task>>
            {
                new List<Task> {new Importer(mockfs.Object, new List<SongPath>{new SongPath("")}, false)},
                new List<Task> {new Identifier(new LocalSong[]{}, null, null, null, null), new Identifier(new LocalSong[]{}, null, null, null, null)}
            };

            var subject = new Importer(mockfs.Object, new List<SongPath> { new SongPath("") }, false);
            TaskAdder.ResolveConflicts(subject, queue);

            Assert.AreEqual(2, queue.Count, "Queue doesn't have the right number of stages!");

            Assert.AreEqual(2, queue[0].Count, "Stage 1 doesn't have the new task!");
            Assert.IsInstanceOf(typeof(Importer), queue[0][0], "Stage 1 doesn't have the old import task!");
            Assert.AreSame(subject, queue[0][1], "Stage 1 doesn't have the subject task!");
            Assert.AreEqual(2, queue[1].Count, "Stage 2 isn't the same size!");
            Assert.IsInstanceOf(typeof(Identifier), queue[1][0], "Stage 2 task 1 isn't an IdentifierTask!");
            Assert.IsInstanceOf(typeof(Identifier), queue[1][1], "Stage 2 task 2 isn't an IdentifierTask!");

        }
    }
}