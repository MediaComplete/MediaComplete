using System.IO;

namespace MSOE.MediaComplete.Test
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

        public static FileInfo CreateTestFile(string path)
        {
            var destPath = path + Path.DirectorySeparatorChar + Constants.ValidMp3FileName;
            if (File.Exists(destPath)) 
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(path);
            File.Copy(Constants.ValidMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateUnknownFile(string path)
        {
            var destPath = path + Path.DirectorySeparatorChar + Constants.ValidMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(path);
            File.Copy(Constants.UnknownMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateNonMp3TestFile(string path)
        {
            var destPath = path + Path.DirectorySeparatorChar + Constants.NonMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(path);
            File.Create(destPath).Close();
            return new FileInfo(destPath);
        }

        public static FileInfo CreateInvalidTestFile(string path)
        {
            var destPath = path + Path.DirectorySeparatorChar + Constants.InvalidMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(path);
            File.Copy(Constants.InvalidMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateMissingAlbumTestFile(string path)
        {
            var destPath = path + Path.DirectorySeparatorChar + Constants.MissingAlbumMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(path);
            File.Copy(Constants.MissingAlbumMp3FullPath, destPath);
            return new FileInfo(destPath);
        }

        public static FileInfo CreateMissingArtistTestFile(string path)
        {
            var destPath = path + Path.DirectorySeparatorChar + Constants.MissingArtistMp3FileName;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(path);
            File.Copy(Constants.MissingArtistMp3FullPath, destPath);
            return new FileInfo(destPath);
        }
    }
}
