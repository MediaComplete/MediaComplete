using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class SorterTest
    {
        private static readonly List<MetaAttribute> _sortOrder = new List<MetaAttribute>
        {
            MetaAttribute.Artist,
            MetaAttribute.Album
        };
        private static readonly DirectoryPath _homeDir = new DirectoryPath("homedir");

        #region CalculateActions

        [TestMethod]
        public void CalculateActions_MovesOnly()
        {
            var manager = SetUpCalculateActionsMock();
            var songs = new List<SongPath>{
               new SongPath(SettingWrapper.MusicDir.FullPath + "song4.mp3"),
               new SongPath(SettingWrapper.MusicDir.FullPath + "song5.mp3"),
               new SongPath(SettingWrapper.MusicDir.FullPath + "song6.mp3")};
            var sorter = new Sorter(manager, songs);
            sorter.CalculateActionsAsync().Wait();

            Assert.AreEqual(3, sorter.Actions.Count());
            Assert.AreEqual(0, sorter.DupCount);
            Assert.AreEqual(3, sorter.MoveCount);
        }

        [TestMethod]
        public void CalculateActions_DuplicateOnly()
        {
            var manager = SetUpCalculateActionsMock();
            var songs = new List<SongPath>{
                new SongPath(SettingWrapper.MusicDir.FullPath + "song8.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath + "song9.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath + "song10.mp3")};
            var sorter = new Sorter(manager, songs);
            sorter.CalculateActionsAsync().Wait();

            Assert.AreEqual(3, sorter.Actions.Count());
            Assert.AreEqual(3, sorter.DupCount);
            Assert.AreEqual(0, sorter.MoveCount);
        }

        [TestMethod]
        public void CalculateActions_MoveAndDuplicate()
        {
            var manager = SetUpCalculateActionsMock();
            var songs = new List<SongPath>{
                new SongPath(SettingWrapper.MusicDir.FullPath + "song4.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath + "song5.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath + "song10.mp3")};

            var sorter = new Sorter(manager, songs);
            sorter.CalculateActionsAsync().Wait();

            Assert.AreEqual(3, sorter.Actions.Count());
            Assert.AreEqual(1, sorter.DupCount);
            Assert.AreEqual(2, sorter.MoveCount);
        }

        [TestMethod]
        public void CalculateActions_NoValidFiles()
        {
            var manager = SetUpCalculateActionsMock();
            var songs = new List<SongPath>{
                new SongPath("song99.mp3"),
                new SongPath("song100.mp3"), 
                new SongPath("song101.mp3")};
            var sorter = new Sorter(manager, songs);
            sorter.CalculateActionsAsync().Wait();

            Assert.IsTrue(!sorter.Actions.Any());
            Assert.AreEqual(0, sorter.DupCount);
            Assert.AreEqual(0, sorter.MoveCount);
        }
        [TestMethod]
        public void CalculateActions_MoveDupAndInvalid()
        {
            var manager = SetUpCalculateActionsMock();
            var songs = new List<SongPath>{
               new SongPath(SettingWrapper.MusicDir.FullPath + "song1.mp3"),
               new SongPath(SettingWrapper.MusicDir.FullPath + "song4.mp3"),
               new SongPath(SettingWrapper.MusicDir.FullPath + "song8.mp3")};

            var sorter = new Sorter(manager, songs);
            sorter.CalculateActionsAsync().Wait();

            Assert.AreEqual(2, sorter.Actions.Count());
            Assert.AreEqual(1, sorter.DupCount);
            Assert.AreEqual(1, sorter.MoveCount);
        }

        [TestMethod]
        public void CalculateActions_AlreadySorted()
        {
            var manager = SetUpCalculateActionsMock();
            var songs = new List<SongPath>{
                new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                "AlbumName"+Path.DirectorySeparatorChar +"song1.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                "AlbumName"+Path.DirectorySeparatorChar +"song2.mp3"),
                new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                "AlbumName"+Path.DirectorySeparatorChar +"song3.mp3")};
            var sorter = new Sorter(manager, songs);
            sorter.CalculateActionsAsync().Wait();

            Assert.AreEqual(0, sorter.Actions.Count());
            Assert.AreEqual(0, sorter.DupCount);
            Assert.AreEqual(0, sorter.MoveCount);
            
        }
        private static IFileManager SetUpCalculateActionsMock()
        {
            SettingWrapper.SortOrder = _sortOrder;
            SettingWrapper.HomeDir = _homeDir;
            var mock = new Mock<IFileManager>();
            var allSongs = new List<LocalSong>{
                new LocalSong("id1", new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                            "AlbumName"+Path.DirectorySeparatorChar +"song1.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id2", new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                            "AlbumName"+Path.DirectorySeparatorChar +"song2.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id3", new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                            "AlbumName"+Path.DirectorySeparatorChar +"song3.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id4", new SongPath(SettingWrapper.MusicDir.FullPath + "song4.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id5", new SongPath(SettingWrapper.MusicDir.FullPath + "song5.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id6", new SongPath(SettingWrapper.MusicDir.FullPath + "song6.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id7", new SongPath(SettingWrapper.MusicDir.FullPath + "song7.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id8", new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                            "AlbumName"+Path.DirectorySeparatorChar +"song8.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id9", new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                            "AlbumName"+Path.DirectorySeparatorChar +"song9.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id10", new SongPath(SettingWrapper.MusicDir.FullPath+"ArtistName"+Path.DirectorySeparatorChar + 
                            "AlbumName"+Path.DirectorySeparatorChar +"song10.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id11", new SongPath(SettingWrapper.MusicDir.FullPath + "song8.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id12", new SongPath(SettingWrapper.MusicDir.FullPath + "song9.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                },
                new LocalSong("id13", new SongPath(SettingWrapper.MusicDir.FullPath + "song10.mp3"))
                {
                    Artist = "ArtistName",
                    Album = "AlbumName"
                }
            };
            mock.Setup(x => x.GetAllSongs()).Returns(allSongs);
            mock.Setup(x => x.FileExists(It.IsIn(allSongs.Select(y => y.SongPath)))).Returns(true);
            return mock.Object;
        }
        #endregion

        
    }
}
