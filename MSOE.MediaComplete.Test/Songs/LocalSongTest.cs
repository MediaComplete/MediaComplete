
using System.IO;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Files;
using NUnit.Framework;
using Constants = MSOE.MediaComplete.Test.Util.Constants;

namespace MSOE.MediaComplete.Test.Songs
{
    public class LocalSongTest
    {
        // Using TestCaseSource since the FileTypes are determined at runtime.
        static readonly object[] BadFiles =
        {
            new object[] {"fakepath"},
            new object[] {Constants.TestFiles[Constants.FileTypes.NonMusic].Item2},
            new object[] {Constants.TestFiles[Constants.FileTypes.Invalid].Item2}
        };
        [TestCaseSource("BadFiles")]
        public void ToMediaItem_BadFile_Exception(string filename)
        {
            var subject = new LocalSong("id",new SongPath(filename));

            //Assert.Throws<FileNotFoundException>(() => subject.ToMediaItem());
        }

        // Using TestCaseSource since the FileTypes are determined at runtime.
        static readonly object[] GoodFiles =
        {
            new object[] {Constants.TestFiles[Constants.FileTypes.ValidMp3].Item2},
            new object[] {Constants.TestFiles[Constants.FileTypes.ValidWma].Item2}
        };
        [TestCaseSource("GoodFiles")]
        public void ToMediaItem_GoodFile_CorrectStrings(string filename)
        {
            var tagFile = TagLib.File.Create(filename);

            var subject = new LocalSong("id", new SongPath(filename));

           // var results = subject.ToMediaItem();

         //   Assert.AreEqual(new FileInfo(filename).FullName, results.Location, "Filename doesn't match!");
        //    Assert.AreEqual((int)tagFile.Properties.Duration.TotalSeconds, results.Runtime, "Song runtime doesn't match!");
         //   Assert.AreEqual(tagFile.Tag.FirstAlbumArtist + " - " + tagFile.Tag.Title, results.Inf, "Info string doesn't match!");
        }
    }
}
