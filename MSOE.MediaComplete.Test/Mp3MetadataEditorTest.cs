using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using System.IO;
using TagLib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class Mp3MetadataEditorTest
    {
        private DirectoryInfo _homeDir;

        [TestInitialize]
        public void Setup()
        {
            _homeDir = Directory.CreateDirectory(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "MP3EditingTestHomeDir");
            FileHelper.CreateTestFile(_homeDir.FullName);
            FileHelper.CreateInvalidTestFile(_homeDir.FullName);
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
        }

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Mp3MetadataEditor_InvalidFileType_ShouldThrowCorruptFileException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Mp3MetadataEditor(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.InvalidMp3FileName);
        }

        [TestMethod]
        public void GetYear_ValidMp3_ShouldReturnYear()
        {
            var subject = new Mp3MetadataEditor(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.ValidMp3FileName);
            Assert.AreEqual((uint)2012, subject.Year);
        }

        [TestMethod]
        public void SetYear_ValidMp3_ShouldChangeYear()
        {
            var subject = new Mp3MetadataEditor(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.ValidMp3FileName);
            Assert.AreEqual((uint)2012, subject.Year);
            subject.Year = 1985;
            Assert.AreEqual((uint)1985, subject.Year);
        }
    }
}
