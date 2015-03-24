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

        #region Append(AbstractSong)

        [TestMethod]
        public void Append_EmptyQueue_HasOneSong()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong")));

            Assert.AreEqual(1, PlayQueue.Inst.Playlist.Songs.Count, "Queue not the right size!");
        }

        [TestMethod]
        public void Append_PopulatedQueue_AddedToEnd()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong")));
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            PlayQueue.Inst.Append(targetSong);

            Assert.AreEqual(2, PlayQueue.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Songs not in the right order!");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Append_NullSong_Exception()
        {
            PlayQueue.Inst.Append((AbstractSong)null);
        }

        #endregion

        #region Append(IEnumerable<AbstractSong>)

        [TestMethod]
        public void Append_EmptyQueue_HasTwoSongs()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                targetSong
            });

            Assert.AreEqual(2, PlayQueue.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Songs not in the right order!");
        }

        [TestMethod]
        public void Append_EmptyList_NoSongsAdded()
        {
            PlayQueue.Inst.Append(new List<AbstractSong>());

            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Queue not the right size!");
        }

        [TestMethod]
        public void Append_PopulatedQueue_SongsAtEnd()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));

            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            PlayQueue.Inst.Append(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                targetSong
            });

            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[2], "Songs not in the right order!");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Append_NullList_Exception()
        {
            PlayQueue.Inst.Append((IEnumerable<AbstractSong>)null);
        }

        #endregion

        #region Remove(int index)

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove_EmptyList_Exception()
        {
            PlayQueue.Inst.Remove(0);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove_NegativeArg_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong")));
            PlayQueue.Inst.Remove(-1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove_TooLargeArg_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong")));
            PlayQueue.Inst.Remove(1);
        }

        [TestMethod]
        public void Remove_ValidArg_QueueShrink()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong")));
            PlayQueue.Inst.Remove(0);

            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Wrong number of songs!");
        }

        #endregion

        #region Remove(AbstractSong)

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullSong_ThrowsException()
        {
            PlayQueue.Inst.Remove((AbstractSong)null);
        }

        [TestMethod]
        public void Remove_SongNotFound_ReturnsFalseAndQueueIntact()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));

            var ret = PlayQueue.Inst.Remove(new LocalSong(new FileInfo("fakesong2")));

            Assert.IsFalse(ret, "Fake song should not have been found.");
            Assert.AreEqual(1, PlayQueue.Inst.Playlist.Songs.Count, "Song should not have been removed");
        }

        [TestMethod]
        public void Remove_QueueEmpty_ReturnsFalse()
        {
            var ret = PlayQueue.Inst.Remove(new LocalSong(new FileInfo("fakesong2")));
            Assert.IsFalse(ret, "Fake song should not have been found.");
            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Queue should still be empty");
        }

        [TestMethod]
        public void Remove_SongFound_ReturnsTrueAndSongRemoved()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));

            var ret = PlayQueue.Inst.Remove(new LocalSong(new FileInfo("fakesong1")));

            Assert.IsTrue(ret, "Fake song should have been found.");
            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Song should have been removed");
        }

        #endregion

        #region Remove(IEnumerable<AbstractSong>)

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullList_Exception()
        {
            PlayQueue.Inst.Remove((IEnumerable<AbstractSong>)null);
        }

        [TestMethod]
        public void Remove_EmptyQueue_NothingHappens()
        {
            PlayQueue.Inst.Remove(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                new LocalSong(new FileInfo("fakesong2"))
            });

            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
        }

        [TestMethod]
        public void Remove_EmptyList_NothingHappens()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Remove(new List<AbstractSong>());

            Assert.AreEqual(1, PlayQueue.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[0], "Song wasn't preserved!");
        }

        [TestMethod]
        public void Remove_SomeExistSomeDoNot_FoundSongsRemoved()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong3"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Remove(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                new LocalSong(new FileInfo("fakesong2"))
            });

            Assert.AreEqual(1, PlayQueue.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[0], "Song wasn't preserved!");
        }

        #endregion

        #region Clear()

        [TestMethod]
        public void Clear_QueueEmpty_NothingHappens()
        {
            PlayQueue.Inst.Clear();

            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
        }

        [TestMethod]
        public void Clear_QueuePopulated_QueueEmptied()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong2")));

            PlayQueue.Inst.Clear();

            Assert.AreEqual(0, PlayQueue.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
        }

        #endregion

        #region Move(int, int)

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_NewIndexNegative_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Move(0, -1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_OldIndexNegative_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Move(-1, 0);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_NewIndexTooBig_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Move(0, 1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_OldIndexTooBig_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Move(1, 0);
        }

        [TestMethod]
        public void Move_OldIndexEqualsNewIndex_NoChange()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong3")));

            PlayQueue.Inst.Move(1, 1);

            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue isn't the right size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Song in the wrong place!");
        }

        [TestMethod]
        public void Move_OldIndexLargerThanNewIndex_MoveBackwards()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong2")));
            var targetSong = new LocalSong(new FileInfo("fakesong3"));
            PlayQueue.Inst.Append(targetSong);
            
            PlayQueue.Inst.Move(2, 1);

            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue isn't the right size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Song in the wrong place!");
        }

        [TestMethod]
        public void Move_OldIndexSmallerThanNewIndex_MoveForwards()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong2")));
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong3")));

            PlayQueue.Inst.Move(0, 1);

            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue isn't the right size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Song in the wrong place!");
        }

        #endregion

        #region Move(AbstractSong, int)

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Move_SongIsNull_Exception()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Move(null, 0);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_IndexIsNegative_Exception()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Move(targetSong, -1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_IndexIsTooBig_Exception()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Move(targetSong, 1);
        }

        [TestMethod]
        public void Move_SongNotFound_ReturnsFalseAndNoChange()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong2")));

            var ret = PlayQueue.Inst.Move(new LocalSong(new FileInfo("fakesong3")), 1);

            Assert.IsFalse(ret, "Song should not have been found!");
            Assert.AreEqual(2, PlayQueue.Inst.Playlist.Songs.Count, "Queue is wrong size!");
        }

        [TestMethod]
        public void Move_SongAlreadyAtIndex_StaysStill()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong3")));

            var ret = PlayQueue.Inst.Move(targetSong, 1);

            Assert.IsTrue(ret, "Song should have been found!");
            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Song wasn't moved!");
        }

        [TestMethod]
        public void Move_SongAheadOfIndex_MovesBack()
        {
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong1")));
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong2")));
            var targetSong = new LocalSong(new FileInfo("fakesong3"));
            PlayQueue.Inst.Append(targetSong);

            var ret = PlayQueue.Inst.Move(targetSong, 1);

            Assert.IsTrue(ret, "Song should have been found!");
            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Song wasn't moved!");
        }

        [TestMethod]
        public void Move_SongBehindIndex_MovesForward()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            PlayQueue.Inst.Append(targetSong);
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong2")));
            PlayQueue.Inst.Append(new LocalSong(new FileInfo("fakesong3")));

            var ret = PlayQueue.Inst.Move(targetSong, 1);

            Assert.IsTrue(ret, "Song should have been found!");
            Assert.AreEqual(3, PlayQueue.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreSame(targetSong, PlayQueue.Inst.Playlist.Songs[1], "Song wasn't moved!");
        }

        #endregion
    }
}
