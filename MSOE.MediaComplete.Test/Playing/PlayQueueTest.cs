using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib.Playing;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete.Test.Playing
{
    [TestClass]
    public class PlayQueueTest
    {
        [TestInitialize]
        public void Init()
        {
            PlayQueue.Inst.Clear();
        }

        #region JumpTo(AbstractSong)
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void JumpTo_NullArgument_ThrowsException()
        {
            PlayQueue.Inst.JumpTo(null);
        }

        [TestMethod]
        public void JumpTo_ArgumentNotInQueue_ReturnsFalseIndexUnchanged()
        {
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                new LocalSong(new FileInfo("notrealfile2"))
            });
            var initialIndex = PlayQueue.Inst.Index;
            var ret = PlayQueue.Inst.JumpTo(new LocalSong(new FileInfo("notrealfile3")));

            Assert.IsFalse(ret, "Return value should have been false for unknown song.");
            Assert.AreEqual(initialIndex, PlayQueue.Inst.Index, "Index should not have moved.");
        }

        [TestMethod]
        public void JumpTo_ArgumentIsPresent_ChangesIndex()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                targetSong
            });
            var initialIndex = PlayQueue.Inst.Index;
            var ret = PlayQueue.Inst.JumpTo(new LocalSong(new FileInfo("notrealfile2")));

            Assert.IsTrue(ret, "Song should have been found.");
            Assert.AreNotEqual(initialIndex, PlayQueue.Inst.Index, "Index should have moved.");
            Assert.AreSame(targetSong, PlayQueue.Inst.CurrentSong(), "Queue is pointing at the wrong song.");
        }
        #endregion

        #region JumpTo(int)
        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void JumpTo_NegativeArgument_Exception()
        {
            PlayQueue.Inst.JumpTo(-1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void JumpTo_ArgumentBiggerThanQueue_Exception()
        {
            PlayQueue.Inst.JumpTo(0);
        }

        [TestMethod]
        public void JumpTo_ArgumentOK_IndexChanges()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                targetSong
            });
            var initialIndex = PlayQueue.Inst.Index;
            var song = PlayQueue.Inst.JumpTo(1);

            Assert.AreNotEqual(initialIndex, PlayQueue.Inst.Index, "Index should have moved.");
            Assert.AreSame(targetSong, song, "Queue is pointing at the wrong song.");
        }
        #endregion

        #region NextSong()
        [TestMethod]
        public void NextSong_EmptyQueue_ReturnsNull()
        {
            var song = PlayQueue.Inst.NextSong();

            Assert.IsNull(song, "Should not have conjured a song");
        }

        [TestMethod]
        public void NextSong_EndOfQueue_ReturnsNull()
        {
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                new LocalSong(new FileInfo("notrealfile2"))
            });
            PlayQueue.Inst.JumpTo(1);
            var song = PlayQueue.Inst.NextSong();

            Assert.IsNull(song, "Should not have conjured a song");
        }

        [TestMethod]
        public void NextSong_BeginningOfQueue_ReturnsNextIndexChanges()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                targetSong,
                new LocalSong(new FileInfo("notrealfile"))
            });
            var initialIndex = PlayQueue.Inst.Index;
            var song = PlayQueue.Inst.NextSong();

            Assert.IsNotNull(song, "Should have returned a song");
            Assert.AreSame(targetSong, song, "Didn't return the right song.");
            Assert.AreEqual(initialIndex + 1, PlayQueue.Inst.Index, "Index didn't increment.");
        }
        #endregion NextSong()

        #region PreviousSong()
        [TestMethod]
        public void PreviousSong_EmptyQueue_ReturnsNull()
        {
            var song = PlayQueue.Inst.PreviousSong();

            Assert.IsNull(song, "Should not have conjured a song");
        }

        [TestMethod]
        public void PreviousSong_BeginningOfQueue_ReturnsNull()
        {
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                new LocalSong(new FileInfo("notrealfile2"))
            });
            var song = PlayQueue.Inst.PreviousSong();

            Assert.IsNull(song, "Should not have conjured a song");
        }

        [TestMethod]
        public void PreviousSong_EndOfQueue_ReturnsPreviousIndexChanges()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                targetSong,
                new LocalSong(new FileInfo("notrealfile"))
            });
            PlayQueue.Inst.JumpTo(1);
            var initialIndex = PlayQueue.Inst.Index;
            var song = PlayQueue.Inst.PreviousSong();

            Assert.IsNotNull(song, "Should have returned a song");
            Assert.AreSame(targetSong, song, "Didn't return the right song.");
            Assert.AreEqual(initialIndex - 1, PlayQueue.Inst.Index, "Index didn't decrement.");
        }
        #endregion

        #region CurrentSong()
        [TestMethod]
        public void CurrentSong_QueueEmpty_ReturnsNull()
        {
            var song = PlayQueue.Inst.CurrentSong();

            Assert.IsNull(song, "Should not have conjured a song");
        }

        [TestMethod]
        public void CurrentSong_QueueNotEmpty_ReturnsSongAtIndex()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                targetSong
            });
            PlayQueue.Inst.NextSong(); // Advance into the queue.

            Assert.AreSame(targetSong, PlayQueue.Inst.CurrentSong(), "Queue is pointing at the wrong song.");
        }
        #endregion
    }
}
