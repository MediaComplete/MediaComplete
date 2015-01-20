using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Test.Util;
using TagLib;
using Constants = MSOE.MediaComplete.Test.Util.Constants;
using File = TagLib.File;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class MetaDataExtensions
    {
        #region Files
        private File _mp3File;
        private static File _file;
        private const string InvalidMp3FileName = "Resources/InvalidMp3File.mp3";
        private const string BlankFile = "Resources/Blanked.mp3";
        private File _file;
        private DirectoryInfo _homeDir;
        private const string InvalidMp3FileName = "Resources/InvalidMp3File.mp3";
        private File _wmaFile;
        private static File _file;
        private const string BlankFile = "Resources/Blanked.mp3";
        private const string ValidMp3FileName = "Resources/ValidMp3File.mp3";
        private const string ValidWmaFileName = "Resources/ValidWmaFile.wma";
        #endregion

     *   #region Attributes
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

        [TestInitialize]
        public void Initialize()
        {
            _homeDir = FileHelper.CreateDirectory("MetadataExtensionsTestHomeDir");
            _file = File.Create(FileHelper.CreateFile(_homeDir, Constants.FileTypes.Blanked).FullName);
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
        }
        #region Initialization
        [ClassCleanup]
        public static void CleanUp()
        {
            _file.SetYear("");
            _file.SetAlbum("");
            _file.SetArtist("");
            _file.SetGenre("");
            _file.SetRating("");
            _file.SetSongTitle("");
            _file.SetSupportingArtists("");
            _file.SetTrack("");

        }
        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            _file = File.Create(BlankFile);
        }
        #endregion

        #region MP3 Getters
        [TestMethod]
        public void GetYear_ValidMp3_ShouldReturnYear()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidYear, _mp3File.GetYear());
        }
        [TestMethod]
        public void GetTrack_ValidMp3_ShouldReturnTrack()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidTrack, _mp3File.GetTrack());
        }
        [TestMethod]
        public void GetTitle_ValidMp3_ShouldReturnTitle()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidTitle, _mp3File.GetSongTitle());
        }
        [TestMethod]
        public void GetAlbum_ValidMp3_ShouldReturnAlbum()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidAlbum, _mp3File.GetAlbum());
        }
        [TestMethod]
        public void GetArtist_ValidMp3_ShouldReturnArtist()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidArtist, _mp3File.GetArtist());
        }
        [TestMethod]
        public void GetSupportingArtist_ValidMp3_ShouldReturnSuppArtist()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidSupportingArtist, _mp3File.GetSupportingArtist());
        }
        [TestMethod]
        public void GetGenre_ValidMp3_ShouldReturnGenre()
        {
            _mp3File = File.Create(ValidMp3FileName);
            Assert.AreEqual(ValidGenre, _mp3File.GetGenre());
        }


        [ClassCleanup]
        public static void CleanUp()
        {
            _file.SetYear("");
            _file.SetAlbum("");
            _file.SetArtist("");
            _file.SetGenre("");
            _file.SetRating("");
            _file.SetSongTitle("");
            _file.SetSupportingArtists("");
            _file.SetTrack("");

        }
        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            _file = File.Create(BlankFile);

        }
        
        #endregion

        #region MP3 Setters
        [TestMethod]
        public void SetYear_BlankMp3_ShouldChangeYear()
        {
            _file.SetYear(ValidYear);
            Assert.AreNotEqual(BadYear, _file.GetYear());
            _file.SetYear(BadYear);
            Assert.AreEqual(BadYear, _file.GetYear());
        }
        [TestMethod]
        public void SetTrack_BlankMp3_ShouldChangeTrack()
        {
            _file.SetTrack(ValidTrack);
            Assert.AreNotEqual(BadTrack, _file.GetTrack());
            _file.SetTrack(BadTrack);
            Assert.AreEqual(BadTrack, _file.GetTrack());
        }
        [TestMethod]
        public void SetTitle_BlankMp3_ShouldChangeTitle()
        {
            _file.SetSongTitle(ValidTitle);
            Assert.AreNotEqual(BadTitle, _file.GetSongTitle());
            _file.SetSongTitle(BadTitle);
            Assert.AreEqual(BadTitle, _file.GetSongTitle());
        }
        [TestMethod]
        public void SetAlbum_BlankMp3_ShouldChangeAlbum()
        {
            _file.SetAlbum(ValidAlbum);
            Assert.AreNotEqual(BadAlbum, _file.GetAlbum());
            _file.SetAlbum(BadAlbum);
            Assert.AreEqual(BadAlbum, _file.GetAlbum());
        }
        [TestMethod]
        public void SetArtist_BlankMp3_ShouldChangeArtist()
        {
            _file.SetArtist(ValidArtist);
            Assert.AreNotEqual(BadArtist, _file.GetArtist());
            _file.SetArtist(BadArtist);
            Assert.AreEqual(BadArtist, _file.GetArtist());
        }
        [TestMethod]
        public void SetSupportingArtist_BlankMp3_ShouldChangeSuppArtist()
        {
            _file.SetSupportingArtists(ValidSupportingArtist);
            Assert.AreNotEqual(BadSupportingArtist, _file.GetSupportingArtist());
            _file.SetSupportingArtists(BadSupportingArtist);
            Assert.AreEqual(BadSupportingArtist, _file.GetSupportingArtist());
        }
        [TestMethod]
        public void SetGenre_BlankMp3_ShouldChangeGenre()
        {
            _file.SetGenre(ValidGenre);
            Assert.AreNotEqual(BadGenre, _file.GetGenre());
            _file.SetGenre(BadGenre);
            Assert.AreEqual(BadGenre, _file.GetGenre());
        }
        #endregion

        #region WMA Getters
        [TestMethod]
        public void GetYear_ValidWma_ShouldReturnYear()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidYear, _wmaFile.GetYear());
        }
        [TestMethod]
        public void GetTrack_ValidWma_ShouldReturnTrack()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidTrack, _wmaFile.GetTrack());
        }
        [TestMethod]
        public void GetTitle_ValidWma_ShouldReturnTitle()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidTitle, _wmaFile.GetSongTitle());
        }
        [TestMethod]
        public void GetAlbum_ValidWma_ShouldReturnAlbum()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidAlbum, _wmaFile.GetAlbum());
        }
        [TestMethod]
        public void GetArtist_ValidWma_ShouldReturnArtist()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidArtist, _wmaFile.GetArtist());
        }
        [TestMethod]
        public void GetSupportingArtist_ValidWma_ShouldReturnSuppArtist()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidSupportingArtist, _wmaFile.GetSupportingArtist());
        }
        [TestMethod]
        public void GetGenre_ValidWma_ShouldReturnGenre()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            Assert.AreEqual(ValidGenre, _wmaFile.GetGenre());
        }
        #endregion

        #region WMA Setters
        [TestMethod]
        public void SetYear_BlankWma_ShouldChangeYear()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetYear(ValidYear);
            Assert.AreNotEqual(BadYear, _wmaFile.GetYear());
            _wmaFile.SetYear(BadYear);
            Assert.AreEqual(BadYear, _wmaFile.GetYear());
        }
        [TestMethod]
        public void SetTrack_BlankWma_ShouldChangeTrack()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetTrack(ValidTrack);
            Assert.AreNotEqual(BadTrack, _wmaFile.GetTrack());
            _wmaFile.SetTrack(BadTrack);
            Assert.AreEqual(BadTrack, _wmaFile.GetTrack());
        }
        [TestMethod]
        public void SetTitle_BlankWma_ShouldChangeTitle()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetSongTitle(ValidTitle);
            Assert.AreNotEqual(BadTitle, _wmaFile.GetSongTitle());
            _wmaFile.SetSongTitle(BadTitle);
            Assert.AreEqual(BadTitle, _wmaFile.GetSongTitle());
        }
        [TestMethod]
        public void SetAlbum_BlankWma_ShouldChangeAlbum()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetAlbum(ValidAlbum);
            Assert.AreNotEqual(BadAlbum, _wmaFile.GetAlbum());
            _wmaFile.SetAlbum(BadAlbum);
            Assert.AreEqual(BadAlbum, _wmaFile.GetAlbum());
        }
        [TestMethod]
        public void SetArtist_BlankWma_ShouldChangeArtist()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetArtist(ValidArtist);
            Assert.AreNotEqual(BadArtist, _wmaFile.GetArtist());
            _wmaFile.SetArtist(BadArtist);
            Assert.AreEqual(BadArtist, _wmaFile.GetArtist());
        }
        [TestMethod]
        public void SetSupportingArtist_BlankWma_ShouldChangeSuppArtist()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetSupportingArtists(ValidSupportingArtist);
            Assert.AreNotEqual(BadSupportingArtist, _wmaFile.GetSupportingArtist());
            _wmaFile.SetSupportingArtists(BadSupportingArtist);
            Assert.AreEqual(BadSupportingArtist, _wmaFile.GetSupportingArtist());
        }
        [TestMethod]
        public void SetGenre_BlankWma_ShouldChangeGenre()
        {
            _wmaFile = File.Create(ValidWmaFileName);
            _wmaFile.SetGenre(ValidGenre);
            Assert.AreNotEqual(BadGenre, _wmaFile.GetGenre());
            _wmaFile.SetGenre(BadGenre);
            Assert.AreEqual(BadGenre, _wmaFile.GetGenre());
        }
        #endregion
    }
}
