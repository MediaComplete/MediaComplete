namespace MSOE.MediaComplete.Test.Util
{
    /// <summary>
    /// Test related constant values
    /// </summary>
    class Constants
    {
        public const string ResourceDirName = "Resources";
        public const string InvalidMp3FileName = "InvalidMp3File.mp3";
        public const string InvalidMp3FullPath = ResourceDirName + "/" + InvalidMp3FileName;
        public const string ValidMp3FileName = "ValidMp3File.mp3";
        public const string ValidMp3FullPath = ResourceDirName + "/" + ValidMp3FileName;
        public const string UnknownMp3FileName = "UnknownMp3.mp3";
        public const string UnknownMp3FullPath = ResourceDirName + "/" + UnknownMp3FileName;
        public const string MissingAlbumMp3FileName = "MissingAlbumMp3File.mp3";
        public const string MissingAlbumMp3FullPath = ResourceDirName + "/" + MissingAlbumMp3FileName;
        public const string MissingArtistMp3FileName = "MissingArtistMp3File.mp3";
        public const string MissingArtistMp3FullPath = ResourceDirName + "/" + MissingArtistMp3FileName;

        public const string NonMp3FileName = "NonMp3TestFile.txt";
        public const string NonMp3FullPath = ResourceDirName + "/" + NonMp3FileName;
    }
}
