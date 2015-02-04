﻿using System.IO;
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
            Assert.AreEqual(ValidYear, _mp3File.GetAttribute(MetaAttribute.Year));
        }
        [TestMethod]
        public void GetTrack_ValidMp3_ShouldReturnTrack()
        {
            Assert.AreEqual(ValidTrack, _mp3File.GetAttribute(MetaAttribute.TrackNumber));
        }
        [TestMethod]
        public void GetTitle_ValidMp3_ShouldReturnTitle()
        {
            Assert.AreEqual(ValidTitle, _mp3File.GetAttribute(MetaAttribute.SongTitle));
        }
        [TestMethod]
        public void GetAlbum_ValidMp3_ShouldReturnAlbum()
        {
            Assert.AreEqual(ValidAlbum, _mp3File.GetAttribute(MetaAttribute.Album));
        }
        [TestMethod]
        public void GetArtist_ValidMp3_ShouldReturnArtist()
        {
            Assert.AreEqual(ValidArtist, _mp3File.GetAttribute(MetaAttribute.Artist));
        }
        [TestMethod]
        public void GetSupportingArtist_ValidMp3_ShouldReturnSuppArtist()
        {
            Assert.AreEqual(ValidSupportingArtist, _mp3File.GetAttribute(MetaAttribute.SupportingArtist));
        }
        [TestMethod]
        public void GetGenre_ValidMp3_ShouldReturnGenre()
        {
            Assert.AreEqual(ValidGenre, _mp3File.GetAttribute(MetaAttribute.Genre));
        }
        #endregion

        #region MP3 Setters
        [TestMethod]
        public void SetYear_BlankMp3_ShouldChangeYear()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Year, ValidYear);
            Assert.AreEqual(ValidYear, _blankMp3File.GetAttribute(MetaAttribute.Year));
            _blankMp3File.SetAttribute(MetaAttribute.Year, BadYear);
            Assert.AreEqual(BadYear, _blankMp3File.GetAttribute(MetaAttribute.Year));
        }
        [TestMethod]
        public void SetTrack_BlankMp3_ShouldChangeTrack()
        {
            _blankMp3File.SetAttribute(MetaAttribute.TrackNumber, ValidTrack);
            Assert.AreEqual(ValidTrack, _blankMp3File.GetAttribute(MetaAttribute.TrackNumber));
            _blankMp3File.SetAttribute(MetaAttribute.TrackNumber, BadTrack);
            Assert.AreEqual(BadTrack, _blankMp3File.GetAttribute(MetaAttribute.TrackNumber));
        }
        [TestMethod]
        public void SetTitle_BlankMp3_ShouldChangeTitle()
        {
            _blankMp3File.SetAttribute(MetaAttribute.SongTitle, ValidTitle);
            Assert.AreEqual(ValidTitle, _blankMp3File.GetAttribute(MetaAttribute.SongTitle));
            _blankMp3File.SetAttribute(MetaAttribute.SongTitle, BadTitle);
            Assert.AreEqual(BadTitle, _blankMp3File.GetAttribute(MetaAttribute.SongTitle));
        }
        [TestMethod]
        public void SetAlbum_BlankMp3_ShouldChangeAlbum()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Album, ValidAlbum);
            Assert.AreEqual(ValidAlbum, _blankMp3File.GetAttribute(MetaAttribute.Album));
            _blankMp3File.SetAttribute(MetaAttribute.Album, BadAlbum);
            Assert.AreEqual(BadAlbum, _blankMp3File.GetAttribute(MetaAttribute.Album));
        }
        [TestMethod]
        public void SetArtist_BlankMp3_ShouldChangeArtist()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Artist, ValidArtist);
            Assert.AreEqual(ValidArtist, _blankMp3File.GetAttribute(MetaAttribute.Artist));
            _blankMp3File.SetAttribute(MetaAttribute.Artist, BadArtist);
            Assert.AreEqual(BadArtist, _blankMp3File.GetAttribute(MetaAttribute.Artist));
        }
        [TestMethod]
        public void SetSupportingArtist_BlankMp3_ShouldChangeSuppArtist()
        {
            _blankMp3File.SetAttribute(MetaAttribute.SupportingArtist, ValidSupportingArtist);
            Assert.AreEqual(ValidSupportingArtist, _blankMp3File.GetAttribute(MetaAttribute.SupportingArtist));
            _blankMp3File.SetAttribute(MetaAttribute.SupportingArtist, BadSupportingArtist);
            Assert.AreEqual(BadSupportingArtist, _blankMp3File.GetAttribute(MetaAttribute.SupportingArtist));
        }
        [TestMethod]
        public void SetGenre_BlankMp3_ShouldChangeGenre()
        {
            _blankMp3File.SetAttribute(MetaAttribute.Genre, ValidGenre);
            Assert.AreEqual(ValidGenre, _blankMp3File.GetAttribute(MetaAttribute.Genre));
            _blankMp3File.SetAttribute(MetaAttribute.Genre, BadGenre);
            Assert.AreEqual(BadGenre, _blankMp3File.GetAttribute(MetaAttribute.Genre));
        }
        #endregion

        #region WMA Getters
        [TestMethod]
        public void GetYear_ValidWma_ShouldReturnYear()
        {
            Assert.AreEqual(ValidYear, _wmaFile.GetAttribute(MetaAttribute.Year));
        }
        [TestMethod]
        public void GetTrack_ValidWma_ShouldReturnTrack()
        {
            Assert.AreEqual(ValidTrack, _wmaFile.GetAttribute(MetaAttribute.TrackNumber));
        }
        [TestMethod]
        public void GetTitle_ValidWma_ShouldReturnTitle()
        {
            Assert.AreEqual(ValidTitle, _wmaFile.GetAttribute(MetaAttribute.SongTitle));
        }
        [TestMethod]
        public void GetAlbum_ValidWma_ShouldReturnAlbum()
        {
            Assert.AreEqual(ValidAlbum, _wmaFile.GetAttribute(MetaAttribute.Album));
        }
        [TestMethod]
        public void GetArtist_ValidWma_ShouldReturnArtist()
        {
            Assert.AreEqual(ValidArtist, _wmaFile.GetAttribute(MetaAttribute.Artist));
        }
        [TestMethod]
        public void GetSupportingArtist_ValidWma_ShouldReturnSuppArtist()
        {
            Assert.AreEqual(ValidSupportingArtist, _wmaFile.GetAttribute(MetaAttribute.SupportingArtist));
        }
        [TestMethod]
        public void GetGenre_ValidWma_ShouldReturnGenre()
        {
            Assert.AreEqual(ValidGenre, _wmaFile.GetAttribute(MetaAttribute.Genre));
        }
        #endregion

        #region WMA Setters
        [TestMethod]
        public void SetYear_BlankWma_ShouldChangeYear()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Year, ValidYear);
            Assert.AreEqual(ValidYear, _blankWmaFile.GetAttribute(MetaAttribute.Year));
            _blankWmaFile.SetAttribute(MetaAttribute.Year, BadYear);
            Assert.AreEqual(BadYear, _blankWmaFile.GetAttribute(MetaAttribute.Year));
        }
        [TestMethod]
        public void SetTrack_BlankWma_ShouldChangeTrack()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.TrackNumber, ValidTrack);
            Assert.AreEqual(ValidTrack, _blankWmaFile.GetAttribute(MetaAttribute.TrackNumber));
            _blankWmaFile.SetAttribute(MetaAttribute.TrackNumber, BadTrack);
            Assert.AreEqual(BadTrack, _blankWmaFile.GetAttribute(MetaAttribute.TrackNumber));
        }
        [TestMethod]
        public void SetTitle_BlankWma_ShouldChangeTitle()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.SongTitle, ValidTitle);
            Assert.AreEqual(ValidTitle, _blankWmaFile.GetAttribute(MetaAttribute.SongTitle));
            _blankWmaFile.SetAttribute(MetaAttribute.SongTitle, BadTitle);
            Assert.AreEqual(BadTitle, _blankWmaFile.GetAttribute(MetaAttribute.SongTitle));
        }
        [TestMethod]
        public void SetAlbum_BlankWma_ShouldChangeAlbum()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Album, ValidAlbum);
            Assert.AreEqual(ValidAlbum, _blankWmaFile.GetAttribute(MetaAttribute.Album));
            _blankWmaFile.SetAttribute(MetaAttribute.Album, BadAlbum);
            Assert.AreEqual(BadAlbum, _blankWmaFile.GetAttribute(MetaAttribute.Album));
        }
        [TestMethod]
        public void SetArtist_BlankWma_ShouldChangeArtist()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Artist, ValidArtist);
            Assert.AreEqual(ValidArtist, _blankWmaFile.GetAttribute(MetaAttribute.Artist));
            _blankWmaFile.SetAttribute(MetaAttribute.Artist, BadArtist);
            Assert.AreEqual(BadArtist, _blankWmaFile.GetAttribute(MetaAttribute.Artist));
        }
        [TestMethod]
        public void SetSupportingArtist_BlankWma_ShouldChangeSuppArtist()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.SupportingArtist, ValidSupportingArtist);
            Assert.AreEqual(ValidSupportingArtist, _blankWmaFile.GetAttribute(MetaAttribute.SupportingArtist));
            _blankWmaFile.SetAttribute(MetaAttribute.SupportingArtist, BadSupportingArtist);
            Assert.AreEqual(BadSupportingArtist, _blankWmaFile.GetAttribute(MetaAttribute.SupportingArtist));
        }
        [TestMethod]
        public void SetGenre_BlankWma_ShouldChangeGenre()
        {
            _blankWmaFile.SetAttribute(MetaAttribute.Genre, ValidGenre);
            Assert.AreEqual(ValidGenre, _blankWmaFile.GetAttribute(MetaAttribute.Genre));
            _blankWmaFile.SetAttribute(MetaAttribute.Genre, BadGenre);
            Assert.AreEqual(BadGenre, _blankWmaFile.GetAttribute(MetaAttribute.Genre));
        }
        #endregion
    }
}
