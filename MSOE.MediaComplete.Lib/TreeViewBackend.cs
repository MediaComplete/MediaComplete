using System.IO;
namespace MSOE.MediaComplete.Lib
{
    public class TreeViewBackend
    {
        public static FileInfo[] GetFiles(string homeDir)
        {
            var rootDirInfo = new DirectoryInfo(homeDir);
            return GetFiles(rootDirInfo);
        }

        public static FileInfo[] GetFiles(DirectoryInfo rootDirInfo)
        {
            return rootDirInfo.GetFiles();
        }
        public static DirectoryInfo[] GetDirectories(string homeDir)
        {
            var rootDirInfo = new DirectoryInfo(homeDir);
            return GetDirectories(rootDirInfo);
        }

        public static DirectoryInfo[] GetDirectories(DirectoryInfo rootDirInfo)
        {
            return rootDirInfo.GetDirectories();
        }
    }
}
