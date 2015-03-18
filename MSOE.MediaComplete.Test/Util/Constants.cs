using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MSOE.MediaComplete.Test.Util
{
    /// <summary>
    /// Test related constant values
    /// </summary>
    public static class Constants
    {
        private const string ResourceDirName = "Resources";
        private const string InvalidMp3FileName = "InvalidMp3File.mp3";
        private const string InvalidMp3FullPath = ResourceDirName + "/" + InvalidMp3FileName;
        private const string ValidMp3FileName = "ValidMp3File.mp3";
        private const string ValidMp3FullPath = ResourceDirName + "/" + ValidMp3FileName;
        private const string UnknownMp3FileName = "UnknownMp3.mp3";
        private const string UnknownMp3FullPath = ResourceDirName + "/" + UnknownMp3FileName;
        private const string BlankedMp3FileName = "BlankedMp3.mp3";
        private const string BlankedMp3FileFullPath = ResourceDirName + "/" + BlankedMp3FileName;
        private const string BlankedWmaFileName = "BlankedWma.wma";
        private const string BlankedWmaFileFullPath = ResourceDirName + "/" + BlankedWmaFileName;
        private const string MissingAlbumMp3FileName = "MissingAlbumMp3File.mp3";
        private const string MissingAlbumMp3FullPath = ResourceDirName + "/" + MissingAlbumMp3FileName;
        private const string MissingArtistMp3FileName = "MissingArtistMp3File.mp3";
        private const string MissingArtistMp3FullPath = ResourceDirName + "/" + MissingArtistMp3FileName;
        private const string NonMusicFileName = "NonMusicTestFile.txt";
        private const string NonMusicFullPath = ResourceDirName + "/" + NonMusicFileName;
        private const string ValidWmaFileName = "ValidWmaFile.wma";
        private const string ValidWmaFullPath = ResourceDirName + "/" + ValidWmaFileName;
        private const string ValidWavFileName = "ValidWavFile.wav";
        private const string ValidWavFullPath = ResourceDirName + "/" + ValidWavFileName;
        
        public enum FileTypes
        {
            ValidMp3, Unknown, BlankedMp3, NonMusic, Invalid, MissingAlbum, MissingArtist, BlankedWma, ValidWma, ValidWav
        }

        public static readonly ReadOnlyDictionary<FileTypes, Tuple<string, string>> TestFiles =
            new ReadOnlyDictionary<FileTypes, Tuple<string, string>>(new Dictionary<FileTypes, Tuple<string, string>>
            {
                { FileTypes.ValidMp3, new Tuple<string, string>(ValidMp3FileName, ValidMp3FullPath)},
                { FileTypes.Invalid, new Tuple<string, string>(InvalidMp3FileName, InvalidMp3FullPath)},
                { FileTypes.NonMusic, new Tuple<string, string>(NonMusicFileName, NonMusicFullPath)},
                { FileTypes.Unknown, new Tuple<string, string>(UnknownMp3FileName, UnknownMp3FullPath)},
                { FileTypes.BlankedMp3, new Tuple<string, string>(BlankedMp3FileName, BlankedMp3FileFullPath)},
                { FileTypes.MissingAlbum, new Tuple<string, string>(MissingAlbumMp3FileName, MissingAlbumMp3FullPath)},
                { FileTypes.MissingArtist, new Tuple<string, string>(MissingArtistMp3FileName, MissingArtistMp3FullPath)},
                { FileTypes.BlankedWma, new Tuple<string, string>(BlankedWmaFileName, BlankedWmaFileFullPath)},
                { FileTypes.ValidWma, new Tuple<string, string>(ValidWmaFileName, ValidWmaFullPath)},
                { FileTypes.ValidWav, new Tuple<string, string>(ValidWavFileName, ValidWavFullPath)}
            });
    }
}
