using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib.Playing;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete.Test.Playing
{
    [TestClass]
    public class NowPlayingTest
    {
        [TestInitialize]
        public void Init()
        {
            NowPlaying.Inst.Clear();
        }

        #region JumpTo(AbstractSong)
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void JumpTo_NullArgument_ThrowsException()
        {
            NowPlaying.Inst.JumpTo(null);
        }

        [TestMethod]
        public void JumpTo_ArgumentNotInQueue_ReturnsFalseIndexUnchanged()
        {
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                new LocalSong(new FileInfo("notrealfile2"))
            });
            var initialIndex = NowPlaying.Inst.Index;
            var ret = NowPlaying.Inst.JumpTo(new LocalSong(new FileInfo("notrealfile3")));

            Assert.IsFalse(ret, "Return value should have been false for unknown song.");
            Assert.AreEqual(initialIndex, NowPlaying.Inst.Index, "Index should not have moved.");
        }

        [TestMethod]
        public void JumpTo_ArgumentIsPresent_ChangesIndex()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                targetSong
            });
            var initialIndex = NowPlaying.Inst.Index;
            var ret = NowPlaying.Inst.JumpTo(new LocalSong(new FileInfo("notrealfile2")));

            Assert.IsTrue(ret, "Song should have been found.");
            Assert.AreNotEqual(initialIndex, NowPlaying.Inst.Index, "Index should have moved.");
            Assert.AreSame(targetSong, NowPlaying.Inst.CurrentSong(), "Queue is pointing at the wrong song.");
        }
        #endregion

        #region JumpTo(int)
        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void JumpTo_NegativeArgument_Exception()
        {
            NowPlaying.Inst.JumpTo(-1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void JumpTo_ArgumentBiggerThanQueue_Exception()
        {
            NowPlaying.Inst.JumpTo(0);
        }

        [TestMethod]
        public void JumpTo_ArgumentOK_IndexChanges()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                targetSong
            });
            var initialIndex = NowPlaying.Inst.Index;
            var song = NowPlaying.Inst.JumpTo(1);

            Assert.AreNotEqual(initialIndex, NowPlaying.Inst.Index, "Index should have moved.");
            Assert.AreSame(targetSong, song, "Queue is pointing at the wrong song.");
        }
        #endregion

        #region NextSong()
        [TestMethod]
        public void NextSong_EmptyQueue_ReturnsNull()
        {
            var song = NowPlaying.Inst.NextSong();

            Assert.IsNull(song, "Should not have conjured a song");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should still be -1.");
        }

        [TestMethod]
        public void NextSong_EndOfQueue_ReturnsNull()
        {
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                new LocalSong(new FileInfo("notrealfile2"))
            });
            NowPlaying.Inst.JumpTo(1);
            var song = NowPlaying.Inst.NextSong();

            Assert.IsNull(song, "Should not have conjured a song");
            Assert.AreEqual(1, NowPlaying.Inst.Index, "Index should still be 1.");
        }

        [TestMethod]
        public void NextSong_BeginningOfQueue_ReturnsNextIndexChanges()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                targetSong
            });
            var initialIndex = NowPlaying.Inst.Index;
            var song = NowPlaying.Inst.NextSong();

            Assert.IsNotNull(song, "Should have returned a song");
            Assert.AreSame(targetSong, song, "Didn't return the right song.");
            Assert.AreEqual(initialIndex + 1, NowPlaying.Inst.Index, "Index didn't increment.");
        }
        #endregion NextSong()

        #region PreviousSong()
        [TestMethod]
        public void PreviousSong_EmptyQueue_ReturnsNull()
        {
            var song = NowPlaying.Inst.PreviousSong();

            Assert.IsNull(song, "Should not have conjured a song");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should still be -1.");
        }

        [TestMethod]
        public void PreviousSong_BeginningOfQueue_ReturnsNull()
        {
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("notrealfile")),
                new LocalSong(new FileInfo("notrealfile2"))
            });
            var song = NowPlaying.Inst.PreviousSong();

            Assert.IsNull(song, "Should not have conjured a song");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should still be 0.");
        }

        [TestMethod]
        public void PreviousSong_EndOfQueue_ReturnsPreviousIndexChanges()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                targetSong,
                new LocalSong(new FileInfo("notrealfile"))
            });
            NowPlaying.Inst.JumpTo(1);
            var initialIndex = NowPlaying.Inst.Index;
            var song = NowPlaying.Inst.PreviousSong();

            Assert.IsNotNull(song, "Should have returned a song");
            Assert.AreSame(targetSong, song, "Didn't return the right song.");
            Assert.AreEqual(initialIndex - 1, NowPlaying.Inst.Index, "Index didn't decrement.");
        }
        #endregion

        #region CurrentSong()
        [TestMethod]
        public void CurrentSong_QueueEmpty_ReturnsNull()
        {
            var song = NowPlaying.Inst.CurrentSong();

            Assert.IsNull(song, "Should not have conjured a song");
        }

        [TestMethod]
        public void CurrentSong_QueueNotEmpty_ReturnsSongAtIndex()
        {
            var targetSong = new LocalSong(new FileInfo("notrealfile2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                targetSong
            });
            NowPlaying.Inst.NextSong(); // Advance into the queue.

            Assert.AreSame(targetSong, NowPlaying.Inst.CurrentSong(), "Queue is pointing at the wrong song.");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should be 0");
        }
        #endregion

        #region HasNextSong()
        [TestMethod]
        public void HasNextSong_AddSimple()
        {
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("firstsong")));
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("secondsong")));
            Assert.IsTrue(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.NextSong();
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.PreviousSong();
            Assert.IsTrue(NowPlaying.Inst.HasNextSong());
        }
        [TestMethod]
        public void HasNextSong_Move()
        {
            var song = new LocalSong(new FileInfo("firstsong"));
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Add(song);
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("song2")));
            Assert.IsTrue(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Move(song, NowPlaying.Inst.SongCount() - 1);
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            
            NowPlaying.Inst.Move(song, 0);
            Console.Out.Write(NowPlaying.Inst.CurrentSong());
            Assert.IsTrue(NowPlaying.Inst.HasNextSong());
        }

        [TestMethod]
        public void HasNextSong_ClearRemove()
        {
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("firstsong")));
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("secondsong")));
            Assert.IsTrue(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Remove(0);
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
            NowPlaying.Inst.Clear();
            Assert.IsFalse(NowPlaying.Inst.HasNextSong());
        }
        #endregion

        #region Add(AbstractSong)

        [TestMethod]
        public void Add_EmptyQueue_HasOneSong()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong")));

            Assert.AreEqual(1, NowPlaying.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Add_PopulatedQueue_AddedToEnd()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong")));
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            NowPlaying.Inst.Add(targetSong);

            Assert.AreEqual(2, NowPlaying.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Songs not in the right order!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullSong_Exception()
        {
            NowPlaying.Inst.Add((AbstractSong)null);
        }

        #endregion

        #region Add(IEnumerable<AbstractSong>)

        [TestMethod]
        public void Add_EmptyQueue_HasTwoSongs()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                targetSong
            });

            Assert.AreEqual(2, NowPlaying.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Songs not in the right order!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Add_EmptyList_NoSongsAdded()
        {
            NowPlaying.Inst.Add(new List<AbstractSong>());

            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have remained at -1.");
        }

        [TestMethod]
        public void Add_PopulatedQueue_SongsAtEnd()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));

            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            NowPlaying.Inst.Add(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                targetSong
            });

            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue not the right size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[2], "Songs not in the right order!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullList_Exception()
        {
            NowPlaying.Inst.Add((IEnumerable<AbstractSong>)null);
        }

        #endregion

        #region Remove(int index)

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove_EmptyList_Exception()
        {
            NowPlaying.Inst.Remove(0);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove_NegativeArg_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong")));
            NowPlaying.Inst.Remove(-1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove_TooLargeArg_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong")));
            NowPlaying.Inst.Remove(1);
        }

        [TestMethod]
        public void Remove_ValidArg_QueueEmpty()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong")));
            NowPlaying.Inst.Remove(0);

            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Wrong number of songs!");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have shrunk to -1.");
        }

        [TestMethod]
        public void Remove_ValidArg_QueueShrink()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));
            NowPlaying.Inst.Remove(0);

            Assert.AreEqual(1, NowPlaying.Inst.Playlist.Songs.Count, "Wrong number of songs!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have shrunk to 0.");
        }

        #endregion

        #region Remove(AbstractSong)

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullSong_ThrowsException()
        {
            NowPlaying.Inst.Remove((AbstractSong)null);
        }

        [TestMethod]
        public void Remove_SongNotFound_ReturnsFalseAndQueueIntact()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));

            var ret = NowPlaying.Inst.Remove(new LocalSong(new FileInfo("fakesong2")));

            Assert.IsFalse(ret, "Fake song should not have been found.");
            Assert.AreEqual(1, NowPlaying.Inst.Playlist.Songs.Count, "Song should not have been removed");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Remove_QueueEmpty_ReturnsFalse()
        {
            var ret = NowPlaying.Inst.Remove(new LocalSong(new FileInfo("fakesong2")));
            Assert.IsFalse(ret, "Fake song should not have been found.");
            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Queue should still be empty");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have remained at -1.");
        }

        [TestMethod]
        public void Remove_SongFound_ReturnsTrueAndQueueEmptied()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));

            var ret = NowPlaying.Inst.Remove(new LocalSong(new FileInfo("fakesong1")));

            Assert.IsTrue(ret, "Fake song should have been found.");
            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Song should have been removed");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have shrunk to -1.");
        }

        [TestMethod]
        public void Remove_SongFound_ReturnsTrueAndSongRemoved()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));

            var ret = NowPlaying.Inst.Remove(new LocalSong(new FileInfo("fakesong1")));

            Assert.IsTrue(ret, "Fake song should have been found.");
            Assert.AreEqual(1, NowPlaying.Inst.Playlist.Songs.Count, "Song should have been removed");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have shrunk to 0.");
        }

        #endregion

        #region Remove(IEnumerable<AbstractSong>)

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullList_Exception()
        {
            NowPlaying.Inst.Remove((IEnumerable<AbstractSong>)null);
        }

        [TestMethod]
        public void Remove_EmptyQueue_NothingHappens()
        {
            NowPlaying.Inst.Remove(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                new LocalSong(new FileInfo("fakesong2"))
            });

            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have remained at -1.");
        }

        [TestMethod]
        public void Remove_EmptyList_NothingHappens()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Remove(new List<AbstractSong>());

            Assert.AreEqual(1, NowPlaying.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[0], "Song wasn't preserved!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Remove_SomeExistSomeDoNot_FoundSongsRemoved()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong3"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Remove(new List<AbstractSong>
            {
                new LocalSong(new FileInfo("fakesong1")),
                new LocalSong(new FileInfo("fakesong2"))
            });

            Assert.AreEqual(1, NowPlaying.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[0], "Song wasn't preserved!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        #endregion

        #region Clear()

        [TestMethod]
        public void Clear_QueueEmpty_NothingHappens()
        {
            NowPlaying.Inst.Clear();

            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have shrunk to -1.");
        }

        [TestMethod]
        public void Clear_QueuePopulated_QueueEmptied()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));

            NowPlaying.Inst.Clear();

            Assert.AreEqual(0, NowPlaying.Inst.Playlist.Songs.Count, "Queue is the wrong size!");
            Assert.AreEqual(-1, NowPlaying.Inst.Index, "Index should have shrunk to -1.");
        }

        #endregion

        #region Move(int, int)

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_NewIndexNegative_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Move(0, -1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_OldIndexNegative_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Move(-1, 0);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_NewIndexTooBig_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Move(0, 1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_OldIndexTooBig_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Move(1, 0);
        }

        [TestMethod]
        public void Move_OldIndexEqualsNewIndex_NoChange()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong3")));

            NowPlaying.Inst.Move(1, 1);

            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue isn't the right size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Song in the wrong place!");

            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Move_OldIndexLargerThanNewIndex_MoveBackwards()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));
            var targetSong = new LocalSong(new FileInfo("fakesong3"));
            NowPlaying.Inst.Add(targetSong);
            
            NowPlaying.Inst.Move(2, 1);

            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue isn't the right size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Song in the wrong place!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Move_OldIndexSmallerThanNewIndex_MoveForwardsAndIndexUpdates()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong3")));

            NowPlaying.Inst.Move(0, 1);

            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue isn't the right size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Song in the wrong place!");
            Assert.AreEqual(1, NowPlaying.Inst.Index, "Index should have shifted to 1.");
        }

        #endregion

        #region Move(AbstractSong, int)

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Move_SongIsNull_Exception()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Move(null, 0);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_IndexIsNegative_Exception()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Move(targetSong, -1);
        }

        [TestMethod, ExpectedException(typeof(IndexOutOfRangeException))]
        public void Move_IndexIsTooBig_Exception()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Move(targetSong, 1);
        }

        [TestMethod]
        public void Move_SongNotFound_ReturnsFalseAndNoChange()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));

            var ret = NowPlaying.Inst.Move(new LocalSong(new FileInfo("fakesong3")), 1);

            Assert.IsFalse(ret, "Song should not have been found!");
            Assert.AreEqual(2, NowPlaying.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Move_SongAlreadyAtIndex_StaysStill()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            var targetSong = new LocalSong(new FileInfo("fakesong2"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong3")));

            var ret = NowPlaying.Inst.Move(targetSong, 1);

            Assert.IsTrue(ret, "Song should have been found!");
            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Song wasn't moved!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Move_SongAheadOfIndex_MovesBack()
        {
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong1")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));
            var targetSong = new LocalSong(new FileInfo("fakesong3"));
            NowPlaying.Inst.Add(targetSong);

            var ret = NowPlaying.Inst.Move(targetSong, 1);

            Assert.IsTrue(ret, "Song should have been found!");
            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Song wasn't moved!");
            Assert.AreEqual(0, NowPlaying.Inst.Index, "Index should have advanced to 0.");
        }

        [TestMethod]
        public void Move_SongBehindIndex_MovesForwardAndIndexUpdates()
        {
            var targetSong = new LocalSong(new FileInfo("fakesong1"));
            NowPlaying.Inst.Add(targetSong);
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong2")));
            NowPlaying.Inst.Add(new LocalSong(new FileInfo("fakesong3")));

            var ret = NowPlaying.Inst.Move(targetSong, 1);

            Assert.IsTrue(ret, "Song should have been found!");
            Assert.AreEqual(3, NowPlaying.Inst.Playlist.Songs.Count, "Queue is wrong size!");
            Assert.AreSame(targetSong, NowPlaying.Inst.Playlist.Songs[1], "Song wasn't moved!");
            Assert.AreEqual(1, NowPlaying.Inst.Index, "Index should have shifted to 1.");
        }

        #endregion
    }
}
