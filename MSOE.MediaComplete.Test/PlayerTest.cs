using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Test.Util;
using NAudio.Wave;
using TagLib;
using Constants = MSOE.MediaComplete.Test.Util.Constants;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class PlayerTest
    {
        private readonly Player _player = Player.Instance;
        private DirectoryInfo _homeDir;

        [TestInitialize]
        public void Before()
        {
            _homeDir = FileHelper.CreateDirectory("PlayerTestHomeDir");
            _player.Stop();
        }

        [TestCleanup]
        public void After()
        {
            _player.Stop();
            Directory.Delete(_homeDir.FullName, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Play_NullFileInfo_ReturnWithoutPlaying()
        {
            _player.Play(null);
            Assert.Fail("Play should have thrown an ArgumentNullException");
        }

        [TestMethod]
        public void Play_Mp3File_ReturnAndContinuePlaying()
        {
            var mp3File = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            _player.Play(mp3File);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod, Ignore]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_InvalidMp3File_ThrowCorruptFileException()
        {
            var invalidFile = FileHelper.CreateFile(_homeDir, Constants.FileTypes.Invalid);
            _player.Play(invalidFile);
            Assert.Fail("Play should throw a CorruptFileException");
        }

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_NotAMusicFile_ThrowCorruptFileException()
        {
            var notAMusicFile = FileHelper.CreateFile(_homeDir, Constants.FileTypes.NonMusic);
            _player.Play(notAMusicFile);
            Assert.Fail("Play should throw a CorruptFileException");
        }

        [TestMethod]
        public void Play_WmaFile_ReturnAndContinuePlaying()
        {
            var wmaFile = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidWma);
            _player.Play(wmaFile);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        public void Play_WavFile_ReturnAndContinuePlaying()
        {
            var wavFile = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidWav);
            _player.Play(wavFile);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        public void Player_MultipleStateChanges()
        {
            var mp3File = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            _player.Play(mp3File);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
            _player.Pause();
            Assert.AreEqual(PlaybackState.Paused, _player.PlaybackState);
            _player.Resume();
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
            _player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, _player.PlaybackState);
            _player.Play(mp3File);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
            _player.Pause();
            Assert.AreEqual(PlaybackState.Paused, _player.PlaybackState);
            _player.Stop();
            Assert.AreEqual(PlaybackState.Stopped, _player.PlaybackState);
        }
    }
}
