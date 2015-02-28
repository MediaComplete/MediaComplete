using System;
using M3U.NET;
using MSOE.MediaComplete.Lib.Songs;
using NUnit.Framework;

namespace MSOE.MediaComplete.Test.Songs
{
    public class AbstractSongTest
    {
        [TestCase]
        public void Create_UnknownPathFormat_Exception()
        {
            var unknownItem = new MediaItem
            {
                Location = "fake path"
            };

            Assert.Throws<FormatException>(() => AbstractSong.Create(unknownItem));
        }

        [TestCase("mp3file.mp3")]
        [TestCase("directory\\mp3file.mp3")]
        [TestCase("C:\\directory\\mp3file.mp3")]
        [TestCase("wmafile.wma")]
        [TestCase("directory\\wmafile.wma")]
        [TestCase("C:\\directory\\wmafile.wma")]
        public void Create_Mp3File_ReturnsLocalSong(string filename)
        {
            var fileItem = new MediaItem
            {
                Location = filename
            };

            var song = AbstractSong.Create(fileItem);

            Assert.IsInstanceOf(typeof(LocalSong), song);
        }
    }
}
