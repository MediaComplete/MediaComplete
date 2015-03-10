﻿using M3U.NET;
using Moq;
using MSOE.MediaComplete.Lib.Playlists;
using NUnit.Framework;
using System.Collections.Generic;
using MSOE.MediaComplete.Test.Util;

namespace MSOE.MediaComplete.Test.Playlists
{
    public class PlaylistTest
    {
        [Test]
        public void Save_EmptyList_Saved()
        {
            const string testName = "Test name";
            var mock = BuildM3UMock(testName, new List<MediaItem>());
            var subject = new Playlist(mock.Object);

            subject.Save();

            mock.Verify(m => m.Save(true), Times.Once);
            Assert.AreEqual(testName, subject.Name, "Name wasn't preserved!");
        }

        [Test]
        public void Save_SomeList_Saved()
        {
            const string testName = "Test name";
            var mock = BuildM3UMock(testName, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });
            var subject = new Playlist(mock.Object);

            subject.Save();

            mock.Verify(m => m.Save(true), Times.Once);
            Assert.AreEqual(testName, subject.Name, "Name wasn't preserved!");
        }

        [Test]
        public void Delete_UnsavedList_CallsDelete()
        {
            const string testName = "Test name";
            var mock = BuildM3UMock(testName, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });
            var subject = new Playlist(mock.Object);

            subject.Delete();

            mock.Verify(m => m.Delete(), Times.Once);
            Assert.AreEqual(testName, subject.Name, "Name wasn't preserved!");
        }

        [Test]
        public void Delete_SavedList_CallsDelete()
        {
            const string testName = "Test name";
            var mock = BuildM3UMock(testName, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });
            var subject = new Playlist(mock.Object);

            subject.Save();
            subject.Delete();

            mock.Verify(m => m.Save(true), Times.Once);
            mock.Verify(m => m.Delete(), Times.Once);
            Assert.AreEqual(testName, subject.Name, "Name wasn't preserved!");
        }

        [Test]
        public void Rename_UnsavedList_CallsRename()
        {
            const string testName = "Test name";
            const string newName = "New name";
            var mock = BuildM3UMock(testName, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });

            // ReSharper disable once UseObjectOrCollectionInitializer
            // Disabled since the set operation is the only thing we actually do with it.
            var subject = new Playlist(mock.Object);

            subject.Name = newName;

            mock.VerifySet(m => m.Name = It.IsAny<string>(), Times.Once);
        }

        [Test]
        public void Rename_SavedList_CallsRename()
        {
            const string testName = "Test name";
            const string newName = "New name";
            var mock = BuildM3UMock(testName, new List<MediaItem> { BuildMediaItem(), BuildMediaItem() });
            var subject = new Playlist(mock.Object);

            subject.Save();
            subject.Name = newName;

            mock.Verify(m => m.Save(true), Times.Once);
            mock.VerifySet(m => m.Name = It.IsAny<string>(), Times.Once);
        }

        private Mock<IM3UFile> BuildM3UMock(string name, List<MediaItem> mediaList)
        {
            var mockFile = new Mock<IM3UFile>();
            mockFile.SetupGet(mock => mock.Name).Returns(name);
            mockFile.SetupGet(mock => mock.Files).Returns(mediaList);
            return mockFile;
        }

        private MediaItem BuildMediaItem()
        {
            return new MediaItem {Location = Constants.TestFiles[Constants.FileTypes.ValidMp3].Item2};
        }
    }
}
