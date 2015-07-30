using System;
using System.IO;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Manages a file path to a song file
    /// </summary>
    public class SongPath
    {
        /// <summary>
        /// Full string Path, including the file's name and file-type
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// Constructor requires a string. There is no purpose for this object without that string
        /// </summary>
        /// <param name="path">The string path to the song</param>
        public SongPath(string path)
        {
            FullPath = path;
        }

        /// <summary>
        /// Substring of Path omitting the file's name and file-type
        /// </summary>
        public DirectoryPath Directory
        {
            get { return new DirectoryPath(FullPath.Substring(0, FullPath.LastIndexOf(Path.DirectorySeparatorChar))); }
        }

        /// <summary>
        /// Substring of Path, only including the name and file-type
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
        /// <param name="parent">directory to compare again</param>
        /// <returns>true if the song is a child of that directory</returns>
        /// <returns>false if the song is not a child of that directory</returns>
        public bool HasParent(DirectoryPath parent)
        {
            return FullPath.StartsWith(parent.FullPath, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares the current path to another's path
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        public override bool Equals(Object obj)
        {
            var otherSong = obj as SongPath;
            return otherSong != null && otherSong.FullPath.Equals(FullPath);
        }

        /// <summary>
        /// Generates a unique hash-code for the <see cref="SongPath" /> object, for comparison purposes
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return string.Format("{0}-{1}", FullPath, Name).GetHashCode();
        }
    }

    /// <summary>
    /// Manages a file path to a directory
    /// </summary>
    public class DirectoryPath
    {
        /// <summary>
        /// Full string Path of the directory
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// Constructor requires a string. There is no purpose for this object without that string
        /// </summary>
        /// <param name="path">The string path of the directory</param>
        public DirectoryPath(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                FullPath = path;
            else
                FullPath = path += Path.DirectorySeparatorChar;
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

        /// <summary>
        /// Gets the length of the file path.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length
        {
            get{return FullPath.Length;}
        }

        /// <summary>
        /// Appends an additional path component to the end of this file path
        /// </summary>
        /// <param name="s">The string file path addition</param>
        public void Append(string s)
        {
            FullPath = FullPath += s;
        }

        /// <summary>
        /// Checks to see if this path ends with the specified string path component
        /// </summary>
        /// <param name="s">The suffix to check for</param>
        /// <returns><c>true</c> if the suffix matches, <c>false</c> otherwise</returns>
        public bool EndsWith(string s)
        {
            return FullPath.EndsWith(s, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares two <see cref="DirectoryPath"/>s based on string equality.
        /// </summary>
        /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as DirectoryPath;
            return FullPath.Equals(other.FullPath) && GetHashCode().Equals(other.GetHashCode());
        }

        /// <summary>
        /// Returns a hash code for this instance. This is based on the string hash code of the path and the path's hash code.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return string.Format("{0}-{1}", FullPath, FullPath.GetHashCode()).GetHashCode();
        }
    }
}
