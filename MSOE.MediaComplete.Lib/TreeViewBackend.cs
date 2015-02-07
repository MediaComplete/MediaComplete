using System.Collections.Generic;
using System.IO;
namespace MSOE.MediaComplete.Lib
{
    public static class TreeViewBackend
    {
        public static IEnumerable<FileInfo> GetFiles()
        {
            var rootDirInfo = new DirectoryInfo(SettingWrapper.GetMusicDir());
            return GetFiles(rootDirInfo);
        }

        public static IEnumerable<FileInfo> GetFiles(DirectoryInfo rootDirInfo)
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
        public static IEnumerable<DirectoryInfo> GetDirectories()
        {
            var rootDirInfo = new DirectoryInfo(SettingWrapper.GetMusicDir());
            return GetDirectories(rootDirInfo);
        }

        public static IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo rootDirInfo)
        {
            return rootDirInfo.GetDirectories();
        }
    }
}
