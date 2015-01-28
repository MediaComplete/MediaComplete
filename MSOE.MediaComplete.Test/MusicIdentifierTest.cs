using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Test.Util;
using Constants = MSOE.MediaComplete.Test.Util.Constants;
using File = TagLib.File;

namespace MSOE.MediaComplete.Test
{
    // TODO MC-139 ignored while we find an Echonest alternative
    [TestClass, Ignore]
    public class MusicIdentifierTest
    {
        private DirectoryInfo _homeDir;
        private File _mp3File;

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
            var file = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            _mp3File = File.Create(file.FullName);
            const string artist = "Not an Artist";
            _mp3File.SetAttribute(MetaAttribute.Artist, artist);
            var task = MusicIdentifier.IdentifySong(_mp3File.Name);

            SpinWait.SpinUntil(() => task.IsCompleted, 30000);

            _mp3File = File.Create(file.FullName);
            Assert.AreNotEqual(artist, _mp3File.GetAttribute(MetaAttribute.Artist), "Name was not fixed!");
        }

        [TestMethod, Timeout(30000)]
        public void Identify_UnknownSong_ReturnsNoData()
        {
            var file = FileHelper.CreateFile(_homeDir, Constants.FileTypes.Unknown);

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
            var file = FileHelper.CreateFile(_homeDir, Constants.FileTypes.Invalid);

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
            var file = FileHelper.CreateFile(_homeDir, Constants.FileTypes.NonMusic);

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
