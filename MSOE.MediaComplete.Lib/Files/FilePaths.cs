using System;
using System.IO;

namespace MSOE.MediaComplete.Lib.Files
{
    public class SongPath
    {
        /// <summary>
        /// Full string Path, including the file's name and filetype
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// Constructor requires a string. There is no purpose for this object without that string
        /// </summary>
        /// <param name="path"></param>
        public SongPath(string path)
        {
            FullPath = path;
        }

        /// <summary>
        /// Substring of Path omitting the file's name and filetype
        /// </summary>
        public DirectoryPath Directory
        {
            get { return new DirectoryPath(FullPath.Substring(0, FullPath.LastIndexOf(Path.DirectorySeparatorChar))); }
        }

        /// <summary>
        /// Substring of Path, only including the name and filetype
        /// </summary>
        public string Name
        {
            get
            {
                return FullPath.Substring(FullPath.LastIndexOf(Path.DirectorySeparatorChar)+1);
            }
        }
        
        /// <summary>
        /// Checks to see if this song is at any level a child of the parent.
        /// </summary>
        /// <param name="parent">directory to compare agains</param>
        /// <returns>true if the song is a child of that directory</returns>
        /// <returns>false if the song is not a child of that directory</returns>
        public bool HasParent(DirectoryPath parent)
        {
            return FullPath.StartsWith(parent.FullPath, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Compares the current path to another's path
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            var otherSong = obj as SongPath;
            return otherSong != null && otherSong.FullPath.Equals(FullPath);
        }
        
        /// <summary>
        /// Generates a unique hashcode for the songpath object, for comparison purposes
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return string.Format("{0}-{1}", FullPath, Name).GetHashCode();
        }
    }
    public class DirectoryPath
    {
        /// <summary>
        /// Full string Path of the directory
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// Constructor requires a string. There is no purpose for this object without that string
        /// </summary>
        /// <param name="path"></param>
        public DirectoryPath(string path)
        {
            FullPath = path;
        }

        /// <summary>
        /// Determines if this directory is a child of a specified directory
        /// </summary>
        /// <param name="parent">the directory to compare against</param>
        /// <returns>true if this directory is a child of that directory</returns>
        /// <returns>false if this directory is not a child of that directory</returns>
        public bool HasParent(DirectoryPath parent)
        {
            return FullPath.StartsWith(parent.FullPath, StringComparison.Ordinal);
        }
    }
}
