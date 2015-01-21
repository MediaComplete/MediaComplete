using System.IO;
namespace MSOE.MediaComplete.Lib
{
    public class TreeViewBackend
    {
        public static FileInfo[] GetFiles()
        {
            var rootDirInfo = new DirectoryInfo(SettingWrapper.GetHomeDir());
            return GetFiles(rootDirInfo);
        }

        public static FileInfo[] GetFiles(DirectoryInfo rootDirInfo)
        {
            try
            {
                return rootDirInfo.GetFiles();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(rootDirInfo.FullName);
                return rootDirInfo.GetFiles();
            }
        }
        public static DirectoryInfo[] GetDirectories()
        {
            var rootDirInfo = new DirectoryInfo(SettingWrapper.GetHomeDir());
            return GetDirectories(rootDirInfo);
        }

        public static DirectoryInfo[] GetDirectories(DirectoryInfo rootDirInfo)
        {
            return rootDirInfo.GetDirectories();
        }
    }
}
