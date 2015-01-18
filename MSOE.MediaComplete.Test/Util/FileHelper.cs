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

        public static FileInfo CreateTestFile(string location)
        {
            var destPath = location + Path.DirectorySeparatorChar + Constants.ValidMp3FileName;
            if (File.Exists(destPath)) 
            {
                File.Delete(destPath);
            }
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);
            File.Copy(Constants.ValidMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateUnknownFile(string location)
        {
            var destPath = location + Path.DirectorySeparatorChar + Constants.UnknownMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location);
            File.Copy(Constants.UnknownMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateNonMp3TestFile(string location)
        {
            var destPath = location + Path.DirectorySeparatorChar + Constants.NonMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location);
            File.Create(destPath).Close();
            return new FileInfo(destPath);
        }

        public static FileInfo CreateInvalidTestFile(string location)
        {
            var destPath = location + Path.DirectorySeparatorChar + Constants.InvalidMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location);
            File.Copy(Constants.InvalidMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateMissingAlbumTestFile(string location)
        {
            var destPath = location + Path.DirectorySeparatorChar + Constants.MissingAlbumMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location);
            File.Copy(Constants.MissingAlbumMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateMissingArtistTestFile(string location)
        {
            var destPath = location + Path.DirectorySeparatorChar + Constants.MissingArtistMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location);
            File.Copy(Constants.MissingArtistMp3FullPath, destPath);
            return new FileInfo(destPath);
        }
    }
}
