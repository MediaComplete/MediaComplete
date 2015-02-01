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
        private Player _player;
        private DirectoryInfo _homeDir;

        [TestInitialize]
        public void Before()
        {
            _player = Player.Instance;
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
        public void Play_NullFileInfo_ReturnWithoutPlaying()
        {
            _player.Play(null);
            Assert.AreEqual(PlaybackState.Stopped, _player.PlaybackState);
        }

        [TestMethod]
        public void Play_Mp3File_ReturnAndContinuePlaying()
        {
            var mp3File = new FileInfo(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3).FullName);
            _player.Play(mp3File);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_InvalidMp3File_ThrowCorruptFileException()
        {
            var invalidFile = new FileInfo(FileHelper.CreateFile(_homeDir, Constants.FileTypes.Invalid).FullName);
            _player.Play(invalidFile);
            Assert.Fail("Play should throw a CorruptFileException");
        }

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Play_NotAMusicFile_ThrowCorruptFileException()
        {
            var notAMusicFile = new FileInfo(FileHelper.CreateFile(_homeDir, Constants.FileTypes.NonMusic).FullName);
            _player.Play(notAMusicFile);
            Assert.Fail("Play should throw a CorruptFileException");
        }

        [TestMethod]
        public void Play_WmaFile_ReturnAndContinuePlaying()
        {
            var wmaFile = new FileInfo(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidWma).FullName);
            _player.Play(wmaFile);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        public void Play_WavFile_ReturnAndContinuePlaying()
        {
            var wavFile = new FileInfo(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidWav).FullName);
            _player.Play(wavFile);
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        public void Player_MultipleStateChanges()
        {
            var mp3File = new FileInfo(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3).FullName);
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
