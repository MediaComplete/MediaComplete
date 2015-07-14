using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using Moq;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using Assert = NUnit.Framework.Assert;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class ImporterTest
    {
        private static readonly DirectoryPath HomeDir = new DirectoryPath("homedir");

        #region Constructor
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullEverything()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Importer(null, null, false);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullFiles()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Importer(new Mock<ILibrary>().Object, null, false);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullFileManager()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Importer(null, new List<SongPath> { new SongPath("") }, false);
        }


        #endregion
        
        #region Do
        [TestMethod]
        public void Do_MoveOnly()
        {
            var manager = SetUpMock();
            var files = new List<SongPath>
            {
                new SongPath("song4.mp3"),
                new SongPath("song5.mp3"),
                new SongPath("song6.mp3")
            };
            var importer = new Importer(manager.Object, files, true);
            importer.Do(1);
            manager.Verify(x => x.AddFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Exactly(3));
            manager.Verify(x => x.CopyFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Never);
            Assert.AreEqual(0, importer.Results.FailCount);
            Assert.AreEqual(3, importer.Results.NewFiles.Count);
        }
        [TestMethod]
        public void Do_CopyOnly()
        {
            var manager = SetUpMock();
            var files = new List<SongPath>
            {
                new SongPath("song4.mp3"),
                new SongPath("song5.mp3"),
                new SongPath("song6.mp3")
            };
            var importer = new Importer(manager.Object, files, false);
            importer.Do(1);
            manager.Verify(x => x.AddFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Never);
            manager.Verify(x => x.CopyFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Exactly(3));
            Assert.AreEqual(0, importer.Results.FailCount);
            Assert.AreEqual(3, importer.Results.NewFiles.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidImportException))]
        public void Do_MoveFromWithinDir()
        {
            var manager = SetUpMock();
            var files = new List<SongPath>
            {
                new SongPath(SettingWrapper.MusicDir.FullPath+"song4.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+"song5.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+"song6.mp3")
            };
            // ReSharper disable once ObjectCreationAsStatement
            new Importer(manager.Object, files, true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidImportException))]
        public void Do_CopyFromWithinDir()
        {
            var manager = SetUpMock();
            var files = new List<SongPath>
            {
                new SongPath(SettingWrapper.MusicDir.FullPath+"song4.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+"song5.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+"song6.mp3")
            };
            // ReSharper disable once ObjectCreationAsStatement
            new Importer(manager.Object, files, false);
        }

        [TestMethod]
        public void Do_MoveAlreadyExists()
        {
            var manager = SetUpMock();
            var files = new List<SongPath>
            {
                new SongPath("song1.mp3"),
                new SongPath("song2.mp3"),
                new SongPath("song3.mp3")
            };
            var importer = new Importer(manager.Object, files, true);
            importer.Do(1);
            manager.Verify(x => x.AddFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Never);
            manager.Verify(x => x.CopyFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Never);
            Assert.AreEqual(0, importer.Results.FailCount);
            Assert.AreEqual(0, importer.Results.NewFiles.Count);
        }
        [TestMethod]
        public void Do_CopyAlreadyExists()
        {
            var manager = SetUpMock();
            var files = new List<SongPath>
            {
                new SongPath("song1.mp3"),
                new SongPath("song2.mp3"),
                new SongPath("song3.mp3")
            };
            var importer = new Importer(manager.Object, files, false);
            importer.Do(1);
            manager.Verify(x => x.AddFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Never);
            manager.Verify(x => x.CopyFile(It.IsAny<SongPath>(), It.IsAny<SongPath>()), Times.Never);
            Assert.AreEqual(0, importer.Results.FailCount);
            Assert.AreEqual(0, importer.Results.NewFiles.Count);
        }
        #endregion

        private static Mock<ILibrary> SetUpMock()
        {
            SettingWrapper.HomeDir = HomeDir;
            var mock = new Mock<ILibrary>();
            var allSongs = new List<SongPath>{
                new SongPath(SettingWrapper.MusicDir.FullPath+ "song1.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+ "song2.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+ "song3.mp3")
            };
            mock.Setup(x => x.FileExists(It.IsIn<SongPath>(allSongs))).Returns(true);

            return mock;
        }
    }
}
