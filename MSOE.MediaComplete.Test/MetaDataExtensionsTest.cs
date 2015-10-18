using System.IO;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Test.Util;
using NUnit.Framework;
using Constants = MSOE.MediaComplete.Test.Util.Constants;
using File = TagLib.File;

namespace MSOE.MediaComplete.Test
{
    [TestFixture]
    public class MetadataExtensionsTest
    {
        #region Files
        private static File _blankMp3File;
        private static File _blankWmaFile;
        private File _mp3File;
        private File _wmaFile;
        private DirectoryInfo _homeDir;
        #endregion

        #region Attributes
        private const string ValidYear = "2012";
        private const string ValidTrack = "1";
        private const string ValidTitle = "Get Got";
        private const string ValidAlbum = "The Money Store";
        private const string ValidArtist = "Death Grips";
        private const string ValidSupportingArtist = "Supporting Artist";
        private const string ValidGenre = "Rock";
        private const string BadYear = "2992";
        private const string BadTrack = "8";
        private const string BadTitle = "NotTitle!";
        private const string BadAlbum = "NotAlbum";
        private const string BadArtist = "NotArtist";
        private const string BadSupportingArtist = "artist12,artist32";
        private const string BadGenre = "BadGenre";
        #endregion

        #region Initialization
        [SetUp]
        public void Initialize()
        {
            _homeDir = FileHelper.CreateDirectory("MetadataExtensionsTestHomeDir");
            _blankMp3File = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.BlankedMp3).FullName);
            _blankWmaFile = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.BlankedWma).FullName);
            _mp3File = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3).FullName);
            _wmaFile = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidWma).FullName); 
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
        }
        #endregion

        #region MP3 Getters
        [Test]
        public void GetYear_ValidMp3_ShouldReturnYear()
        {
            Assert.AreEqual(ValidYear, _mp3File.GetAttribute(MetaAttribute.Year));
        }
        [Test]
        public void GetTrack_ValidMp3_ShouldReturnTrack()
        {
            Assert.AreEqual(ValidTrack, _mp3File.GetAttribute(MetaAttribute.TrackNumber));
        }
        [Test]
        public void GetTitle_ValidMp3_ShouldReturnTitle()
        {
            Assert.AreEqual(ValidTitle, _mp3File.GetAttribute(MetaAttribute.SongTitle));
        }
        [Test]
        public void GetAlbum_ValidMp3_ShouldReturnAlbum()
        {
            Assert.AreEqual(ValidAlbum, _mp3File.GetAttribute(MetaAttribute.Album));
        }
        [Test]
        public void GetArtist_ValidMp3_ShouldReturnArtist()
        {
            Assert.AreEqual(ValidArtist, ((string[])_mp3File.GetAttribute(MetaAttribute.Artist))[0]);
        }
        [Test]
        public void GetSupportingArtist_ValidMp3_ShouldReturnSuppArtist()
        {
            Assert.AreEqual(ValidSupportingArtist, ((string[])_mp3File.GetAttribute(MetaAttribute.SupportingArtist))[0]);
        }
        [Test]
        public void GetGenre_ValidMp3_ShouldReturnGenre()
        {
            Assert.AreEqual(ValidGenre, ((string[])_mp3File.GetAttribute(MetaAttribute.Genre))[0]);
        }
        #endregion

        #region MP3 Setters
        [Test]
        public void SetYear_BlankMp3_ShouldChangeYear()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Year, ValidYear);
            Assert.AreEqual(ValidYear, _blankMp3File.GetAttribute(MetaAttribute.Year));
            _blankMp3File.SetAttribute(MetaAttribute.Year, BadYear);
            Assert.AreEqual(BadYear, _blankMp3File.GetAttribute(MetaAttribute.Year));
        }
        [Test]
        public void SetTrack_BlankMp3_ShouldChangeTrack()
        {
            _blankMp3File.SetAttribute(MetaAttribute.TrackNumber, ValidTrack);
            Assert.AreEqual(ValidTrack, _blankMp3File.GetAttribute(MetaAttribute.TrackNumber));
            _blankMp3File.SetAttribute(MetaAttribute.TrackNumber, BadTrack);
            Assert.AreEqual(BadTrack, _blankMp3File.GetAttribute(MetaAttribute.TrackNumber));
        }
        [Test]
        public void SetTitle_BlankMp3_ShouldChangeTitle()
        {
            _blankMp3File.SetAttribute(MetaAttribute.SongTitle, ValidTitle);
            Assert.AreEqual(ValidTitle, _blankMp3File.GetAttribute(MetaAttribute.SongTitle));
            _blankMp3File.SetAttribute(MetaAttribute.SongTitle, BadTitle);
            Assert.AreEqual(BadTitle, _blankMp3File.GetAttribute(MetaAttribute.SongTitle));
        }
        [Test]
        public void SetAlbum_BlankMp3_ShouldChangeAlbum()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Album, ValidAlbum);
            Assert.AreEqual(ValidAlbum, _blankMp3File.GetAttribute(MetaAttribute.Album));
            _blankMp3File.SetAttribute(MetaAttribute.Album, BadAlbum);
            Assert.AreEqual(BadAlbum, _blankMp3File.GetAttribute(MetaAttribute.Album));
        }
        [Test]
        public void SetArtist_BlankMp3_ShouldChangeArtist()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Artist, new []{ ValidArtist });
            Assert.AreEqual(ValidArtist, ((string[])_blankMp3File.GetAttribute(MetaAttribute.Artist))[0]);
            _blankMp3File.SetAttribute(MetaAttribute.Artist, new []{ BadArtist });
            Assert.AreEqual(BadArtist, ((string[])_blankMp3File.GetAttribute(MetaAttribute.Artist))[0]);
        }
        [Test]
        public void SetSupportingArtist_BlankMp3_ShouldChangeSuppArtist()
        {
            _blankMp3File.SetAttribute(MetaAttribute.SupportingArtist, new []{ ValidSupportingArtist });
            Assert.AreEqual(ValidSupportingArtist, ((string[])_blankMp3File.GetAttribute(MetaAttribute.SupportingArtist))[0]);
            _blankMp3File.SetAttribute(MetaAttribute.SupportingArtist, new []{ BadSupportingArtist });
            Assert.AreEqual(BadSupportingArtist, ((string[])_blankMp3File.GetAttribute(MetaAttribute.SupportingArtist))[0]);
        }
        [Test]
        public void SetGenre_BlankMp3_ShouldChangeGenre()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Genre, new []{ ValidGenre });
            Assert.AreEqual(ValidGenre, ((string[])_blankMp3File.GetAttribute(MetaAttribute.Genre))[0]);
            _blankMp3File.SetAttribute(MetaAttribute.Genre, new []{ BadGenre });
            Assert.AreEqual(BadGenre, ((string[])_blankMp3File.GetAttribute(MetaAttribute.Genre))[0]);
        }
        #endregion

        #region WMA Getters
        [Test]
        public void GetYear_ValidWma_ShouldReturnYear()
        {
            Assert.AreEqual(ValidYear, _wmaFile.GetAttribute(MetaAttribute.Year));
        }
        [Test]
        public void GetTrack_ValidWma_ShouldReturnTrack()
        {
            Assert.AreEqual(ValidTrack, _wmaFile.GetAttribute(MetaAttribute.TrackNumber));
        }
        [Test]
        public void GetTitle_ValidWma_ShouldReturnTitle()
        {
            Assert.AreEqual(ValidTitle, _wmaFile.GetAttribute(MetaAttribute.SongTitle));
        }
        [Test]
        public void GetAlbum_ValidWma_ShouldReturnAlbum()
        {
            Assert.AreEqual(ValidAlbum, _wmaFile.GetAttribute(MetaAttribute.Album));
        }
        [Test]
        public void GetArtist_ValidWma_ShouldReturnArtist()
        {
            Assert.AreEqual(ValidArtist, ((string[])_wmaFile.GetAttribute(MetaAttribute.Artist))[0]);
        }
        [Test]
        public void GetSupportingArtist_ValidWma_ShouldReturnSuppArtist()
        {
            Assert.AreEqual(ValidSupportingArtist, ((string[])_wmaFile.GetAttribute(MetaAttribute.SupportingArtist))[0]);
        }
        [Test]
        public void GetGenre_ValidWma_ShouldReturnGenre()
        {
            Assert.AreEqual(ValidGenre, ((string[])_wmaFile.GetAttribute(MetaAttribute.Genre))[0]);
        }
        #endregion

        #region WMA Setters
        [Test]
        public void SetYear_BlankWma_ShouldChangeYear()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Year, ValidYear);
            Assert.AreEqual(ValidYear, _blankWmaFile.GetAttribute(MetaAttribute.Year));
            _blankWmaFile.SetAttribute(MetaAttribute.Year, BadYear);
            Assert.AreEqual(BadYear, _blankWmaFile.GetAttribute(MetaAttribute.Year));
        }
        [Test]
        public void SetTrack_BlankWma_ShouldChangeTrack()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.TrackNumber, ValidTrack);
            Assert.AreEqual(ValidTrack, _blankWmaFile.GetAttribute(MetaAttribute.TrackNumber));
            _blankWmaFile.SetAttribute(MetaAttribute.TrackNumber, BadTrack);
            Assert.AreEqual(BadTrack, _blankWmaFile.GetAttribute(MetaAttribute.TrackNumber));
        }
        [Test]
        public void SetTitle_BlankWma_ShouldChangeTitle()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.SongTitle, ValidTitle);
            Assert.AreEqual(ValidTitle, _blankWmaFile.GetAttribute(MetaAttribute.SongTitle));
            _blankWmaFile.SetAttribute(MetaAttribute.SongTitle, BadTitle);
            Assert.AreEqual(BadTitle, _blankWmaFile.GetAttribute(MetaAttribute.SongTitle));
        }
        [Test]
        public void SetAlbum_BlankWma_ShouldChangeAlbum()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Album, ValidAlbum);
            Assert.AreEqual(ValidAlbum, _blankWmaFile.GetAttribute(MetaAttribute.Album));
            _blankWmaFile.SetAttribute(MetaAttribute.Album, BadAlbum);
            Assert.AreEqual(BadAlbum, _blankWmaFile.GetAttribute(MetaAttribute.Album));
        }
        [Test]
        public void SetArtist_BlankWma_ShouldChangeArtist()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Artist, new[] { ValidArtist });
            Assert.AreEqual(ValidArtist, ((string[])_blankWmaFile.GetAttribute(MetaAttribute.Artist))[0]);
            _blankWmaFile.SetAttribute(MetaAttribute.Artist, new[] { BadArtist });
            Assert.AreEqual(BadArtist, ((string[])_blankWmaFile.GetAttribute(MetaAttribute.Artist))[0]);
        }
        [Test]
        public void SetSupportingArtist_BlankWma_ShouldChangeSuppArtist()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.SupportingArtist, new[] { ValidSupportingArtist });
            Assert.AreEqual(ValidSupportingArtist, ((string[])_blankWmaFile.GetAttribute(MetaAttribute.SupportingArtist))[0]);
            _blankWmaFile.SetAttribute(MetaAttribute.SupportingArtist, new[] { BadSupportingArtist });
            Assert.AreEqual(BadSupportingArtist, ((string[])_blankWmaFile.GetAttribute(MetaAttribute.SupportingArtist))[0]);
        }
        [Test]
        public void SetGenre_BlankWma_ShouldChangeGenre()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Genre, new[] { ValidGenre });
            Assert.AreEqual(ValidGenre, ((string[])_blankWmaFile.GetAttribute(MetaAttribute.Genre))[0]);
            _blankWmaFile.SetAttribute(MetaAttribute.Genre, new[] { BadGenre });
            Assert.AreEqual(BadGenre, ((string[])_blankWmaFile.GetAttribute(MetaAttribute.Genre))[0]);
        }
        #endregion
    }
}
