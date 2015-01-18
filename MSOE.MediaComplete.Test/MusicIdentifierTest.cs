using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using System.IO;
using System.Text.RegularExpressions;
using MSOE.MediaComplete.Test.Util;
using File = TagLib.File;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class MusicIdentifierTest
    {
        private DirectoryInfo _homeDir;
        private File _mp3File;
        private const string ValidMp3FileName = "Resources/ValidMp3File.mp3";

        [TestInitialize]
        public void Setup()
        {
            _homeDir = FileHelper.CreateDirectory("IdentifierTestHomeDir");
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
        }

        [TestMethod]
        public void Identify_KnownSong_RestoresName()
        {
            _mp3File = File.Create(ValidMp3FileName);
            const string artist = "Not an Artist";
            _mp3File.SetArtist(artist);
            var task = MusicIdentifier.IdentifySong(_mp3File.Name);
            while (!task.IsCompleted)
            {
            }

            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreNotEqual(artist, _mp3File.GetArtist(), "Name was not fixed!");
        }

        [TestMethod, Timeout(30000)]
        public void Identify_UnknownSong_ReturnsNoData()
        {
            var file = FileHelper.CreateUnknownFile(_homeDir.FullName);

            var task = MusicIdentifier.IdentifySong(file.FullName);
            while (!task.IsCompleted)
            {
            }

            var equal = 0 == String.Compare("{\"status\":{\"version\":\"4.2\",\"code\":0,\"message\":\"Success\"},\"songs\":[]}",
                Regex.Replace(task.Result, @"\s", ""), StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(equal, "Identifier returned non-empty string for the unknown file!");
        }

        [TestMethod, Timeout(30000)]
        public void Identify_NonexistantSong_ReturnsNull()
        {
            var task = MusicIdentifier.IdentifySong(_homeDir.FullName + Path.DirectorySeparatorChar + "doesnotexist.mp3");
            while (!task.IsCompleted)
            {
            }

            Assert.IsNull(task.Result, "Identifying a nonexistant file returned a non-null result!");
        }

        [TestMethod, Timeout(30000), Ignore] // Test is ignored pending completion of bug MC-107
        public void Identify_CorruptedFile_ThrowsException()
        {
            var file = FileHelper.CreateInvalidTestFile(_homeDir.FullName);

            var task = MusicIdentifier.IdentifySong(file.FullName);
            while (!task.IsCompleted && !task.IsFaulted)
            {
            }

            Assert.IsTrue(task.IsFaulted, "Identification didn't throw an exception.");
            Assert.AreEqual(typeof(IdentificationException), task.Exception != null ? task.Exception.InnerException.GetType() : null, 
                "Identification threw the wrong kind of exception.");
        }

        [TestMethod, Timeout(30000), Ignore] // Test is ignored pending completion of bug MC-107
        public void Identify_NonMP3File_ThrowsException()
        {
            var file = FileHelper.CreateNonMp3TestFile(_homeDir.FullName);

            var task = MusicIdentifier.IdentifySong(file.FullName);
            while (!task.IsCompleted && !task.IsFaulted)
            {
            }

            Assert.IsTrue(task.IsFaulted, "Identification didn't throw an exception.");
            Assert.AreEqual(typeof(IdentificationException), task.Exception != null ? task.Exception.InnerException.GetType() : null,
                "Identification threw the wrong kind of exception.");
        }
    }
}
