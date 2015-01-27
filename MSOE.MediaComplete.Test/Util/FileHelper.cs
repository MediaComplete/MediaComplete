using System.IO;

namespace MSOE.MediaComplete.Test.Util
{
    /// <summary>
    /// This class contains helper methods for managing test files.
    /// </summary>
    static class FileHelper
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

        public static FileInfo CreateFile(DirectoryInfo location, Constants.FileTypes source)
        {
            var destPath = location.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[source].Item1;
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            Directory.CreateDirectory(location.FullName);
            File.Copy(Constants.TestFiles[source].Item2, destPath);
            return new FileInfo(destPath);
        }
    }
}
