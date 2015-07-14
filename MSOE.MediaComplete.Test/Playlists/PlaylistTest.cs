using M3U.NET;
using Moq;
using MSOE.MediaComplete.Lib.Playlists;
using NUnit.Framework;
using System.Collections.Generic;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Test.Util;

namespace MSOE.MediaComplete.Test.Playlists
{
    public class PlaylistTest
    {
        [Test]
        public void Save_EmptyList_Saved()
        {
            const string testTitle = "Test title";
            var mock = BuildM3UMock(testTitle, new List<MediaItem>());
            var subject = new Playlist(mock.Object);

            subject.Save();

            mock.Verify(m => m.Save(true), Times.Once);
            Assert.AreEqual(testTitle, subject.Title, "Title wasn't preserved!");
        }

        [Test]
        public void Save_SomeList_Saved()
        {
            const string testTitle = "Test title";
            var mock = BuildM3UMock(testTitle, new List<MediaItem> { BuildMediaItem(), BuildMediaItem1() });
            var mockManager = new Mock<ILibrary>();

            var song = new LocalSong("id", new SongPath("path"));
            mockManager.Setup(x => x.GetSong(It.IsAny<MediaItem>())).Returns(song);
            var service = new PlaylistServiceImpl(mockManager.Object);

            PlaylistService.SetService(service);

            var subject = new Playlist(mock.Object);

            subject.Save();

            mock.Verify(m => m.Save(true), Times.Once);
            Assert.AreEqual(testTitle, subject.Title, "Title wasn't preserved!");
        }

        [Test]
        public void Delete_UnsavedList_CallsDelete()
        {
            const string testTitle = "Test title";
            var mock = BuildM3UMock(testTitle, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });
            var subject = new Playlist(mock.Object);

            subject.Delete();

            mock.Verify(m => m.Delete(), Times.Once);
            Assert.AreEqual(testTitle, subject.Title, "Title wasn't preserved!");
        }

        [Test]
        public void Delete_SavedList_CallsDelete()
        {
            const string testTitle = "Test title";
            var mock = BuildM3UMock(testTitle, new List<MediaItem> { BuildMediaItem(), BuildMediaItem1() });
            var mockManager = new Mock<ILibrary>();

            var song = new LocalSong("id", new SongPath("path"));
            mockManager.Setup(x => x.GetSong(It.IsAny<MediaItem>())).Returns(song);
            var service = new PlaylistServiceImpl(mockManager.Object);

            PlaylistService.SetService(service);
            var subject = new Playlist(mock.Object);

            subject.Save();
            subject.Delete();

            mock.Verify(m => m.Save(true), Times.Once);
            mock.Verify(m => m.Delete(), Times.Once);
            Assert.AreEqual(testTitle, subject.Title, "Title wasn't preserved!");
        }

        [Test]
        public void Rename_UnsavedList_CallsRename()
        {
            const string testTitle = "Test title";
            const string newTitle = "New title";
            var mock = BuildM3UMock(testTitle, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });

            // ReSharper disable once UseObjectOrCollectionInitializer
            // Disabled since the set operation is the only thing we actually do with it.
            var subject = new Playlist(mock.Object);

            subject.Title = newTitle;

            mock.VerifySet(m => m.Name = It.IsAny<string>(), Times.Once);
        }

        [Test]
        public void Rename_SavedList_CallsRename()
        {
            const string testTitle = "Test title";
            const string newTitle = "New title";
            var mock = BuildM3UMock(testTitle, new List<MediaItem> { BuildMediaItem(), BuildMediaItem1() });

            var mockManager = new Mock<ILibrary>();
            var song = new LocalSong("id", new SongPath("path"));
            mockManager.Setup(x => x.GetSong(It.IsAny<MediaItem>())).Returns(song);
            var service = new PlaylistServiceImpl(mockManager.Object);

            PlaylistService.SetService(service);
            var subject = new Playlist(mock.Object);

            subject.Save();
            subject.Title = newTitle;

            mock.Verify(m => m.Save(true), Times.Once);
            mock.VerifySet(m => m.Name = It.IsAny<string>(), Times.Once);
        }

        private static Mock<IM3UFile> BuildM3UMock(string name, List<MediaItem> mediaList)
        {
            var mockFile = new Mock<IM3UFile>();
            mockFile.SetupGet(mock => mock.Name).Returns(name);
            mockFile.SetupGet(mock => mock.Files).Returns(mediaList);
            return mockFile;
        }

        private static MediaItem BuildMediaItem()
        {
            return new MediaItem { Location = Constants.TestFiles[Constants.FileTypes.ValidMp3].Item2 };
        }
        private static MediaItem BuildMediaItem1()
        {
            return new MediaItem { Location = Constants.TestFiles[Constants.FileTypes.ValidWav].Item2 };
        }
    }
}
