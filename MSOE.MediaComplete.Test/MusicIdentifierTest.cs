using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Metadata;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Library;
using MSOE.MediaComplete.Lib.Library.FileSystem;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class MusicIdentifierTest
    {
        private Mock<IFileSystem> _fileManagerMock;
        private Mock<IAudioReader> _audioReaderMock;
        private Mock<IAudioIdentifier> _audioIdentifierMock;
        private Mock<IMetadataRetriever> _metadataRetrieverMock;

        [TestInitialize]
        public void Setup()
        {
            _fileManagerMock = new Mock<IFileSystem>();
            _fileManagerMock.Setup(m => m.FileExists(It.IsAny<SongPath>())).Returns(true);

            var audioBytes = new byte[] {0x00, 0x12, 0x34, 0x56};

            _audioReaderMock = new Mock<IAudioReader>();
            _audioReaderMock.Setup(m => m.ReadBytesAsync(It.IsAny<LocalSong>(), It.IsAny<int>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(audioBytes));

            _audioIdentifierMock = new Mock<IAudioIdentifier>();
            _audioIdentifierMock.Setup(m => m.IdentifyAsync(audioBytes, It.IsAny<LocalSong>()))
                .Returns(Task.FromResult((object) null)).Callback<byte[], LocalSong>((b, s) =>
                {
                    s.Title = "Title";
                    s.Artists = new[]{"Artist"};
                    s.Album = "Album";
                });

            _metadataRetrieverMock = new Mock<IMetadataRetriever>();
            _metadataRetrieverMock.Setup(m => m.GetMetadataAsync(It.IsAny<LocalSong>()))
                .Returns(Task.FromResult((object)null)).Callback<LocalSong>(s =>
                {
                    s.TrackNumber = 1;
                    s.SupportingArtists = new[] { "SupportingArtist" };
                    s.Genres = new[] {"Genre"};
                    s.Rating = 1;
                    s.Year = 1;
                });
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Identify_NullList_ThrowsException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Identifier(null, _audioReaderMock.Object, _audioIdentifierMock.Object,
                _metadataRetrieverMock.Object, _fileManagerMock.Object);
        }

        [TestMethod]
        public void Identify_EmptyList_DoesNothing()
        {
            var subject = new Identifier(new List<LocalSong>(), _audioReaderMock.Object, _audioIdentifierMock.Object,
                _metadataRetrieverMock.Object, _fileManagerMock.Object);

            subject.Do(1);
            _audioReaderMock.Verify(m => 
                m.ReadBytesAsync(It.IsAny<LocalSong>(), It.IsAny<int>(), It.IsAny<uint>()), Times.Never);
            _audioIdentifierMock.Verify(m =>
                m.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<LocalSong>()), Times.Never);
            _metadataRetrieverMock.Verify(m =>
                m.GetMetadataAsync(It.IsAny<LocalSong>()), Times.Never);
        }


        [TestMethod]
        public void Identify_NullSong_Skip()
        {
            var song1 = new LocalSong("id1", new SongPath("path1"));
            var subject = new Identifier(new List<LocalSong>
            {
                song1, null, song1
            }, _audioReaderMock.Object, _audioIdentifierMock.Object,
                _metadataRetrieverMock.Object, _fileManagerMock.Object);

            subject.Do(1);

            _audioReaderMock.Verify(m =>
                m.ReadBytesAsync(It.IsAny<LocalSong>(), It.IsAny<int>(), It.IsAny<uint>()), Times.Exactly(2));
            _audioIdentifierMock.Verify(m =>
                m.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<LocalSong>()), Times.Exactly(2));
            _metadataRetrieverMock.Verify(m =>
                m.GetMetadataAsync(It.IsAny<LocalSong>()), Times.Exactly(2));
            _fileManagerMock.Verify(m => m.SaveSong(It.IsAny<LocalSong>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Identify_SongDoesNotExist_Skip()
        {
            var song1 = new LocalSong("id1", new SongPath("path1"));
            var song2 = new LocalSong("id2", new SongPath("path2"));
            var subject = new Identifier(new List<LocalSong>
            {
                song2, song1, song2
            }, _audioReaderMock.Object, _audioIdentifierMock.Object,
                _metadataRetrieverMock.Object, _fileManagerMock.Object);

            _fileManagerMock.Setup(m => m.FileExists(It.Is<SongPath>(s => s.Equals(song1.SongPath))))
                .Returns(false);

            subject.Do(1);

            _audioReaderMock.Verify(m =>
                m.ReadBytesAsync(It.IsAny<LocalSong>(), It.IsAny<int>(), It.IsAny<uint>()), Times.Exactly(2));
            _audioIdentifierMock.Verify(m =>
                m.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<LocalSong>()), Times.Exactly(2));
            _metadataRetrieverMock.Verify(m =>
                m.GetMetadataAsync(It.IsAny<LocalSong>()), Times.Exactly(2));
            _fileManagerMock.Verify(m => m.SaveSong(It.IsAny<LocalSong>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Identify_UnknownSong_NoMetaRetrieval()
        {
            var song1 = new LocalSong("id1", new SongPath("path1"));
            var subject = new Identifier(new List<LocalSong>
            {
                song1
            }, _audioReaderMock.Object, _audioIdentifierMock.Object,
                _metadataRetrieverMock.Object, _fileManagerMock.Object);

            // No callback; "Title" will be null so metadata shouldn't be called
            _audioIdentifierMock.Setup(m => 
                m.IdentifyAsync(It.IsAny<byte[]>(), It.Is<LocalSong>(s => s.Id == song1.Id)));

            subject.Do(1);

            _audioReaderMock.Verify(m =>
                m.ReadBytesAsync(It.IsAny<LocalSong>(), It.IsAny<int>(), It.IsAny<uint>()), Times.Once);
            _audioIdentifierMock.Verify(m =>
                m.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<LocalSong>()), Times.Once);
            _metadataRetrieverMock.Verify(m =>
                m.GetMetadataAsync(It.IsAny<LocalSong>()), Times.Never);
            _fileManagerMock.Verify(m => m.SaveSong(It.IsAny<LocalSong>()), Times.Never);
        }

        [TestMethod]
        public void Identify_IdentifierOverrun_CutShort()
        {
            var song1 = new LocalSong("id1", new SongPath("path1"));
            var song2 = new LocalSong("id2", new SongPath("path2"));
            var subject = new Identifier(new List<LocalSong>
            {
                song2, song1, song2
            }, _audioReaderMock.Object, _audioIdentifierMock.Object,
                _metadataRetrieverMock.Object, _fileManagerMock.Object);

            _audioIdentifierMock.Setup(m => 
                m.IdentifyAsync(It.IsAny<byte[]>(), It.Is<LocalSong>(s => s.Equals(song1))))
                .Throws(new IdentificationException("message"));

            subject.Do(1);

            _audioReaderMock.Verify(m =>
                m.ReadBytesAsync(It.IsAny<LocalSong>(), It.IsAny<int>(), It.IsAny<uint>()), Times.Exactly(2));
            _audioIdentifierMock.Verify(m =>
                m.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<LocalSong>()), Times.Exactly(2));
            _metadataRetrieverMock.Verify(m =>
                m.GetMetadataAsync(It.IsAny<LocalSong>()), Times.Once);
            _fileManagerMock.Verify(m => m.SaveSong(It.IsAny<LocalSong>()), Times.Once);
        }
    }
}
