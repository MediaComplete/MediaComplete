using System.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using TagLib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class MetaDataExtensions
    {
        [TestMethod]
        public void PassingTest()
        {
            return;
        }

        [TestMethod]
        public void FailingTest()
        {
            Assert.AreEqual(1, 2);
        }

        /*
        //TODO this was the old metadata test...needs to be refactored -- issues accessing the extension code...
        private Mp3MetadataEditor _mp3;
        private TagLib.File _mp3File;
        private const string InvalidMp3FileName = "Resources/InvalidMp3File.mp3";
        private const string ValidMp3FileName = "Resources/ValidMp3File.mp3";

        [TestMethod]
        [ExpectedException(typeof(CorruptFileException))]
        public void Mp3MetadataEditor_InvalidFileType_ShouldThrowCorruptFileException()
        {
            _mp3File = File.Create(InvalidMp3FileName);
        }

        [TestMethod]
        public void GetYear_ValidMp3_ShouldReturnYear()
        {
            _mp3File = File.Create(ValidMp3FileName);
            //_mp3File
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
         */
    }
        

}
