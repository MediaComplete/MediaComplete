
using System;
using System.Collections.Generic;
using System.IO;

namespace MSOE.MediaComplete.Lib
{
    public interface IPath
    {
        string FullPath { get; }
        bool HasParent(DirectoryPath parent);
    }

    public class SongPath : IPath
    {
        public string FullPath { get; private set; }

        public string Name
        {
            get
            {
                var split = FullPath.Split(Path.DirectorySeparatorChar);
                return split[split.Length - 1];
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
        
    }
    public class DirectoryPath : IPath
    {
        public string FullPath { get; private set; }
        public IEnumerable<SongPath> Songs { get; private set; }
        public DirectoryPath(string path, IEnumerable<SongPath> songs)
        {
            FullPath = path;
            Songs = songs;
        }

        public bool HasParent(DirectoryPath parent)
        {
            return FullPath.StartsWith(parent.FullPath, StringComparison.Ordinal);
        }
    }
}
