using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Test.Util;
using Constants = MSOE.MediaComplete.Test.Util.Constants;
using File = TagLib.File;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
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
        private const string ValidSupportingArtist = "";
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
        [TestInitialize]
        public void Initialize()
        {
            _homeDir = FileHelper.CreateDirectory("MetadataExtensionsTestHomeDir");
            _blankMp3File = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.BlankedMp3).FullName);
            _blankWmaFile = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.BlankedWma).FullName);
            _mp3File = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3).FullName);
            _wmaFile = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidWma).FullName); 
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
        }
        #endregion

        #region MP3 Getters
        [TestMethod]
        public void GetYear_ValidMp3_ShouldReturnYear()
        {
            Assert.AreEqual(ValidYear, _mp3File.GetYear());
        }
        [TestMethod]
        public void GetTrack_ValidMp3_ShouldReturnTrack()
        {
            Assert.AreEqual(ValidTrack, _mp3File.GetTrack());
        }
        [TestMethod]
        public void GetTitle_ValidMp3_ShouldReturnTitle()
        {
            Assert.AreEqual(ValidTitle, _mp3File.GetSongTitle());
        }
        [TestMethod]
        public void GetAlbum_ValidMp3_ShouldReturnAlbum()
        {
            Assert.AreEqual(ValidAlbum, _mp3File.GetAlbum());
        }
        [TestMethod]
        public void GetArtist_ValidMp3_ShouldReturnArtist()
        {
            Assert.AreEqual(ValidArtist, _mp3File.GetArtist());
        }
        [TestMethod]
        public void GetSupportingArtist_ValidMp3_ShouldReturnSuppArtist()
        {
            Assert.AreEqual(ValidSupportingArtist, _mp3File.GetSupportingArtist());
        }
        [TestMethod]
        public void GetGenre_ValidMp3_ShouldReturnGenre()
        {
            Assert.AreEqual(ValidGenre, _mp3File.GetGenre());
        }
        #endregion

        #region MP3 Setters
        [TestMethod]
        public void SetYear_BlankMp3_ShouldChangeYear()
        {
            _blankMp3File.SetYear(ValidYear);
            Assert.AreEqual(ValidYear, _blankMp3File.GetYear());
            _blankMp3File.SetYear(BadYear);
            Assert.AreEqual(BadYear, _blankMp3File.GetYear());
        }
        [TestMethod]
        public void SetTrack_BlankMp3_ShouldChangeTrack()
        {
            _blankMp3File.SetTrack(ValidTrack);
            Assert.AreEqual(ValidTrack, _blankMp3File.GetTrack());
            _blankMp3File.SetTrack(BadTrack);
            Assert.AreEqual(BadTrack, _blankMp3File.GetTrack());
        }
        [TestMethod]
        public void SetTitle_BlankMp3_ShouldChangeTitle()
        {
            _blankMp3File.SetSongTitle(ValidTitle);
            Assert.AreEqual(ValidTitle, _blankMp3File.GetSongTitle());
            _blankMp3File.SetSongTitle(BadTitle);
            Assert.AreEqual(BadTitle, _blankMp3File.GetSongTitle());
        }
        [TestMethod]
        public void SetAlbum_BlankMp3_ShouldChangeAlbum()
        {
            _blankMp3File.SetAlbum(ValidAlbum);
            Assert.AreEqual(ValidAlbum, _blankMp3File.GetAlbum());
            _blankMp3File.SetAlbum(BadAlbum);
            Assert.AreEqual(BadAlbum, _blankMp3File.GetAlbum());
        }
        [TestMethod]
        public void SetArtist_BlankMp3_ShouldChangeArtist()
        {
            _blankMp3File.SetArtist(ValidArtist);
            Assert.AreEqual(ValidArtist, _blankMp3File.GetArtist());
            _blankMp3File.SetArtist(BadArtist);
            Assert.AreEqual(BadArtist, _blankMp3File.GetArtist());
        }
        [TestMethod]
        public void SetSupportingArtist_BlankMp3_ShouldChangeSuppArtist()
        {
            _blankMp3File.SetSupportingArtists(ValidSupportingArtist);
            Assert.AreEqual(ValidSupportingArtist, _blankMp3File.GetSupportingArtist());
            _blankMp3File.SetSupportingArtists(BadSupportingArtist);
            Assert.AreEqual(BadSupportingArtist, _blankMp3File.GetSupportingArtist());
        }
        [TestMethod]
        public void SetGenre_BlankMp3_ShouldChangeGenre()
        {
            _blankMp3File.SetGenre(ValidGenre);
            Assert.AreEqual(ValidGenre, _blankMp3File.GetGenre());
            _blankMp3File.SetGenre(BadGenre);
            Assert.AreEqual(BadGenre, _blankMp3File.GetGenre());
        }
        #endregion

        #region WMA Getters
        [TestMethod]
        public void GetYear_ValidWma_ShouldReturnYear()
        {
            Assert.AreEqual(ValidYear, _wmaFile.GetYear());
        }
        [TestMethod]
        public void GetTrack_ValidWma_ShouldReturnTrack()
        {
            Assert.AreEqual(ValidTrack, _wmaFile.GetTrack());
        }
        [TestMethod]
        public void GetTitle_ValidWma_ShouldReturnTitle()
        {
            Assert.AreEqual(ValidTitle, _wmaFile.GetSongTitle());
        }
        [TestMethod]
        public void GetAlbum_ValidWma_ShouldReturnAlbum()
        {
            Assert.AreEqual(ValidAlbum, _wmaFile.GetAlbum());
        }
        [TestMethod]
        public void GetArtist_ValidWma_ShouldReturnArtist()
        {
            Assert.AreEqual(ValidArtist, _wmaFile.GetArtist());
        }
        [TestMethod]
        public void GetSupportingArtist_ValidWma_ShouldReturnSuppArtist()
        {
            Assert.AreEqual(ValidSupportingArtist, _wmaFile.GetSupportingArtist());
        }
        [TestMethod]
        public void GetGenre_ValidWma_ShouldReturnGenre()
        {
            Assert.AreEqual(ValidGenre, _wmaFile.GetGenre());
        }
        #endregion

        #region WMA Setters
        [TestMethod]
        public void SetYear_BlankWma_ShouldChangeYear()
        {
            _blankWmaFile.SetYear(ValidYear);
            Assert.AreEqual(ValidYear, _blankWmaFile.GetYear());
            _blankWmaFile.SetYear(BadYear);
            Assert.AreEqual(BadYear, _blankWmaFile.GetYear());
        }
        [TestMethod]
        public void SetTrack_BlankWma_ShouldChangeTrack()
        {
            _blankWmaFile.SetTrack(ValidTrack);
            Assert.AreEqual(ValidTrack, _blankWmaFile.GetTrack());
            _blankWmaFile.SetTrack(BadTrack);
            Assert.AreEqual(BadTrack, _blankWmaFile.GetTrack());
        }
        [TestMethod]
        public void SetTitle_BlankWma_ShouldChangeTitle()
        {
            _blankWmaFile.SetSongTitle(ValidTitle);
            Assert.AreEqual(ValidTitle, _blankWmaFile.GetSongTitle());
            _blankWmaFile.SetSongTitle(BadTitle);
            Assert.AreEqual(BadTitle, _blankWmaFile.GetSongTitle());
        }
        [TestMethod]
        public void SetAlbum_BlankWma_ShouldChangeAlbum()
        {
            _blankWmaFile.SetAlbum(ValidAlbum);
            Assert.AreEqual(ValidAlbum, _blankWmaFile.GetAlbum());
            _blankWmaFile.SetAlbum(BadAlbum);
            Assert.AreEqual(BadAlbum, _blankWmaFile.GetAlbum());
        }
        [TestMethod]
        public void SetArtist_BlankWma_ShouldChangeArtist()
        {
            _blankWmaFile.SetArtist(ValidArtist);
            Assert.AreEqual(ValidArtist, _blankWmaFile.GetArtist());
            _blankWmaFile.SetArtist(BadArtist);
            Assert.AreEqual(BadArtist, _blankWmaFile.GetArtist());
        }
        [TestMethod]
        public void SetSupportingArtist_BlankWma_ShouldChangeSuppArtist()
        {
            _blankWmaFile.SetSupportingArtists(ValidSupportingArtist);
            Assert.AreEqual(ValidSupportingArtist, _blankWmaFile.GetSupportingArtist());
            _blankWmaFile.SetSupportingArtists(BadSupportingArtist);
            Assert.AreEqual(BadSupportingArtist,_blankWmaFile.GetSupportingArtist());
        }
        [TestMethod]
        public void SetGenre_BlankWma_ShouldChangeGenre()
        {
            _blankWmaFile.SetGenre(ValidGenre);
            Assert.AreEqual(ValidGenre, _blankWmaFile.GetGenre());
            _blankWmaFile.SetGenre(BadGenre);
            Assert.AreEqual(BadGenre, _blankWmaFile.GetGenre());
        }
        #endregion
    }
}
