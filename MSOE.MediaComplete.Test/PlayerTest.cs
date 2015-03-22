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
            const PlaybackState playingState = PlaybackState.Playing;
            const PlaybackState pausedState = PlaybackState.Paused;
            const PlaybackState stoppedState = PlaybackState.Stopped;
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var mockFile = new FileInfo(stuff);

            mockNAudioWrapper.Setup(m => m.Setup(mockFile, It.IsAny<EventHandler<StoppedEventArgs>>(),1.0));

            var player = new Player(mockNAudioWrapper.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(playingState);

            player.Play(mockFile);
            Assert.AreEqual(playingState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(pausedState);

            player.Pause();
            Assert.AreEqual(pausedState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(playingState);

            player.Resume();
            Assert.AreEqual(playingState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(stoppedState);

            player.Stop();
            Assert.AreEqual(stoppedState, player.PlaybackState);
        }

        [TestMethod]
        public void StateChanges_WeirdPath()
        {
            const PlaybackState playingState = PlaybackState.Playing;
            const PlaybackState pausedState = PlaybackState.Paused;
            const PlaybackState stoppedState = PlaybackState.Stopped;
            var mockNAudioWrapper = new Mock<INAudioWrapper>();
            var stuff = new Fixture().Create<string>();
            var mockFile = new FileInfo(stuff);

            mockNAudioWrapper.Setup(m => m.Setup(mockFile, It.IsAny<EventHandler<StoppedEventArgs>>(),1.0));

            var player = new Player(mockNAudioWrapper.Object);
            mockNAudioWrapper.Setup(m => m.Play()).Returns(playingState);

            player.Play(mockFile);
            Assert.AreEqual(playingState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(stoppedState);

            player.Stop();
            Assert.AreEqual(stoppedState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Stop()).Returns(stoppedState);

            player.Stop();
            Assert.AreEqual(stoppedState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(stoppedState);

            player.Pause();
            Assert.AreEqual(stoppedState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(stoppedState);

            player.Resume();
            Assert.AreEqual(stoppedState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Play()).Returns(playingState);

            player.Play(mockFile);
            Assert.AreEqual(playingState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(pausedState);

            player.Pause();
            Assert.AreEqual(pausedState, player.PlaybackState);

            mockNAudioWrapper.Setup(m => m.Pause()).Returns(pausedState);

            player.Pause();
            Assert.AreEqual(pausedState, player.PlaybackState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Play_NullFileInfo_ThrowsArgumentNullException()
        {
            var mockNAudioWrapper = new Mock<INAudioWrapper>();

            mockNAudioWrapper.Setup(m => m.Setup(null, It.IsAny<EventHandler<StoppedEventArgs>>(), 1.0)).Throws(new ArgumentNullException());

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

            mockNAudioWrapper.Setup(m => m.Setup(anyInvalidFile, It.IsAny<EventHandler<StoppedEventArgs>>(), 1.0)).Throws(new CorruptFileException());

            var player = new Player(mockNAudioWrapper.Object);
            player.Play(anyInvalidFile);

            Assert.Fail("Play should not have handled the CorruptFileException");
        }
    }
}
