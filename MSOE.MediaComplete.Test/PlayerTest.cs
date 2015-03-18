using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSOE.MediaComplete.Lib.Playing;
using NAudio.Wave;
using Ploeh.AutoFixture;
using TagLib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class PlayerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Play_NullFileInfo_ThrowArgumentNullException()
        {
            var player = Player.Instance;
            player.Play(null);
            Assert.Fail("Play should have thrown an ArgumentNullException.");
        }

        [TestMethod]
        public void StateChanges_HappyPath()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var mockFile = new FileInfo(stuff);

            mockNAudioWrapper.Setup(m => m.Setup(mockFile, It.IsAny<EventHandler<StoppedEventArgs>>()));

            var player = new Player(mockNAudioWrapper.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            player.Play(mockFile);
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
            var mockFile = new FileInfo(stuff);

            mockNAudioWrapper.Setup(m => m.Setup(mockFile, It.IsAny<EventHandler<StoppedEventArgs>>()));

            var player = new Player(mockNAudioWrapper.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(PlaybackState.Playing);

            player.Play(mockFile);
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

            player.Play(mockFile);
            Assert.AreEqual(PlaybackState.Playing, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(PlaybackState.Paused);

            player.Pause();
            Assert.AreEqual(PlaybackState.Paused, player.PlaybackState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Play_NullFileInfo_ThrowsArgumentNullException()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();

            mockNAudioWrapper.Setup(m => m.Setup(null, It.IsAny<EventHandler<StoppedEventArgs>>())).Throws(new ArgumentNullException());

            var player = new Player(mockNAudioWrapper.Object);
            player.Play(null);

            Assert.Fail("Play should not have handled the ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_InvalidFileInfo_ThrowsCorruptFileException()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var anyInvalidFile = new FileInfo(stuff);

            mockNAudioWrapper.Setup(m => m.Setup(anyInvalidFile, It.IsAny<EventHandler<StoppedEventArgs>>())).Throws(new CorruptFileException());

            var player = new Player(mockNAudioWrapper.Object);
            player.Play(anyInvalidFile);

            Assert.Fail("Play should not have handled the CorruptFileException");
        }
    }
}
