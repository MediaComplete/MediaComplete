using System;
using System.IO;

namespace MSOE.MediaComplete.Lib.Files
{
    public interface IPath
    {
        string FullPath { get; }
        bool HasParent(DirectoryPath parent);
    }

    public class SongPath : IPath
    {
        public string FullPath { get; private set; }

        public DirectoryPath Directory
        {
            get { return new DirectoryPath(FullPath.Substring(0, FullPath.LastIndexOf(Path.DirectorySeparatorChar))); }
        }

        public string Name
        {
            get
            {
                return FullPath.Substring(FullPath.LastIndexOf(Path.DirectorySeparatorChar)+1);
            }
        }

        public SongPath(string path)
        {
            FullPath = path;
        }
        public bool HasParent(DirectoryPath parent)
        {
            return FullPath.StartsWith(parent.FullPath, StringComparison.Ordinal);
        }
        public override bool Equals(Object obj)
        {
            var otherSong = obj as SongPath;
            return otherSong != null && otherSong.FullPath.Equals(FullPath);
        }
        public override int GetHashCode()
        {
            return string.Format("{0}-{1}", FullPath, Name).GetHashCode();
        }
    }
    public class DirectoryPath : IPath
    {
        public string FullPath { get; private set; }
        public DirectoryPath(string path)
        {
            FullPath = path;
        }

        public bool HasParent(DirectoryPath parent)
        {
            return FullPath.StartsWith(parent.FullPath, StringComparison.Ordinal);
        }
    }
}
