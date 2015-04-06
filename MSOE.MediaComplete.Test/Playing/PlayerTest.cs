using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSOE.MediaComplete.Lib.Playing;
using NAudio.Wave;
using Ploeh.AutoFixture;
using TagLib;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete.Test.Playing
{
    [TestClass]
    public class PlayerTest
    {
        [TestInitialize]
        public void Setup()
        {
            NowPlaying.Inst.Clear();
        }

        [TestMethod]
        public void StateChanges_HappyPath()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var mockFile = new LocalSong(new FileInfo(stuff));

            mockNAudioWrapper.Setup(m => m.Setup(mockFile.File, It.IsAny<EventHandler<StoppedEventArgs>>(),1.0));

            var player = new Player(mockNAudioWrapper.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            NowPlaying.Inst.Add(mockFile);
            player.Play();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            player.Resume();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(PlaybackState.Stopped);

            player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);
        }

        [TestMethod]
        public void StateChanges_WeirdPath()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var mockFile = new LocalSong(new FileInfo(stuff));

            mockNAudioWrapper.Setup(m => m.Setup(mockFile.File, It.IsAny<EventHandler<StoppedEventArgs>>(),1.0));

            var player = new Player(mockNAudioWrapper.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            NowPlaying.Inst.Add(mockFile);
            player.Play();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(PlaybackState.Stopped);

            player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(PlaybackState.Stopped);

            player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Stopped);

            player.Pause();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Stopped);

            player.Resume();
            Assert.AreEqual(PlaybackState.Stopped, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            player.Play();
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);
        }

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_InvalidFileInfo_ThrowsCorruptFileException()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var anyInvalidFile = new LocalSong(new FileInfo(stuff));

            mockNAudioWrapper.Setup(m => m.Setup(anyInvalidFile.File, It.IsAny<EventHandler<StoppedEventArgs>>(),1.0)).Throws(new CorruptFileException());

            var player = new Player(mockNAudioWrapper.Object);
            NowPlaying.Inst.Add(anyInvalidFile);
            player.Play();

            Assert.Fail("Play should not have handled the CorruptFileException");
        }

        //[TestMethod]
        //[ExpectedException(typeof(CorruptFileException))]
        //public void Seek_ProperTimeSpan_PropagatesAsExpected()
        //{
        //    var mockNAudioWrapper = new Mock<INAudioWrapper>();
        //    var stuff = new Fixture().Create<string>();
        //    var anyValidFile = new FileInfo(stuff);
        //
        //    mockNAudioWrapper.Setup(m => m.Setup(anyValidFile, It.IsAny<EventHandler<StoppedEventArgs>>()));
        //
        //    var player = new Player(mockNAudioWrapper.Object);
        //    player.Play(anyValidFile);
        //
        //    var seekTime = TimeSpan.FromSeconds(55);
        //
        //    mockNAudioWrapper.Setup(m => m.Seek(seekTime)).Returns(PlaybackState.Playing);
        //    
        //    player.Seek(seekTime);
        //
        //    //assert what?
        //}
    }
}
