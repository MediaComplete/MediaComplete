using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using NAudio.Wave;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class PlayerTest
    {
        private readonly Player _player = Player.Instance;

        [TestInitialize]
        public void Before()
        {
            _player.Stop();
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
            var player = Player.Instance;
            player.Play(new FileInfo("someMp3.mp3"));//TODO: put an actual file here
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        public void Play_WmaFile_ReturnAndContinuePlaying()
        {
            var player = Player.Instance;
            player.Play(new FileInfo("someWma.wma"));//TODO: put an actual file here
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        [TestMethod]
        public void Play_WavFile_ReturnAndContinuePlaying()
        {
            var player = Player.Instance;
            player.Play(new FileInfo("someWav.wav"));//TODO: put an actual file here
            Assert.AreEqual(PlaybackState.Playing, _player.PlaybackState);
        }

        //TODO: adding
    }
}
