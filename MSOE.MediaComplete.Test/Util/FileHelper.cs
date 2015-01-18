using System.IO;

namespace MSOE.MediaComplete.Test.Util
{
    /// <summary>
    /// This class contains helper methods for managing test files.
    /// </summary>
    class FileHelper
    {
        public static DirectoryInfo CreateDirectory(string path)
        {
            var fullPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + path;
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            return Directory.CreateDirectory(fullPath);
        }

        public static FileInfo CreateTestFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.ValidMp3FullPath));
        }

        public static FileInfo CreateUnknownFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.UnknownMp3FullPath));
        }

        public static FileInfo CreateBlankedFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.BlankedFullPath));
        }

        public static FileInfo CreateNonMp3TestFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.NonMusicFullPath));
        }

        public static FileInfo CreateInvalidTestFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.InvalidMp3FullPath));
        }

        public static FileInfo CreateMissingAlbumTestFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.MissingAlbumMp3FullPath));
        }

        public static FileInfo CreateMissingArtistTestFile(DirectoryInfo location)
        {
            return CreateFile(location, new FileInfo(Constants.MissingArtistMp3FullPath));
        }

        private static FileInfo CreateFile(DirectoryInfo location, FileInfo source)
        {
            var destPath = location.FullName + Path.DirectorySeparatorChar + source.Name;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location.FullName);
            File.Copy(source.FullName, destPath);
            return new FileInfo(destPath);
        }
    }
}
