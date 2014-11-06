using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using TagLib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class Mp3MetadataEditorTest
    {
        private Mp3MetadataEditor _mp3;
        private const string InvalidMp3FileName = "Resources/InvalidMp3File.mp3";
        private const string ValidMp3FileName = "Resources/ValidMp3File.mp3";

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Mp3MetadataEditor_InvalidFileType_ShouldThrowCorruptFileException()
        {
            _mp3 = new Mp3MetadataEditor(InvalidMp3FileName);
        }

        [TestMethod]
        public void GetYear_ValidMp3_ShouldReturnYear()
        {
            _mp3 = new Mp3MetadataEditor(ValidMp3FileName);
            Assert.AreEqual((uint)2012, _mp3.Year);
        }

        [TestMethod]
        public void SetYear_ValidMp3_ShouldChangeYear()
        {
            _mp3 = new Mp3MetadataEditor(ValidMp3FileName);
            Assert.AreEqual((uint)2012, _mp3.Year);
            _mp3.Year = 1985;
            Assert.AreEqual((uint)1985, _mp3.Year);
        }
    }


}
