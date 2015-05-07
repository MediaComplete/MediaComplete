
using System;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Implementation of AbstractSong for local files. This is used for MP3 and WMA files.
    /// </summary>
    public class LocalSong : AbstractSong
    {
        /// <summary>
        /// Constructs a LocalSong from a file
        /// </summary>
        /// <param name="id">The media item</param>
        /// <param name="songPath">The file</param>id
        public LocalSong(string id, SongPath songPath)
        {
            _id = id;
            SongPath = songPath;
        }
        private readonly string _id;
        public override string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Checks if the song is a child of the specified directory
        /// </summary>
        /// <param name="dir">directory to be compared</param>
        /// <returns>true if song is a child</returns>
        /// <returns>false if song is not a child</returns>
        public bool HasParent(DirectoryPath dir)
        {
            return SongPath.HasParent(dir);
        }
        
        #region Path and File Information
        /// <summary>
        /// Contains path information relating to the song
        /// </summary>
        public SongPath SongPath { get; set; }
        /// <summary>
        /// The FileType, as defined by the full name of the file. This is required to save files properly.
        /// </summary>
        public string FileType { get { return Path.Substring(Path.LastIndexOf(".", StringComparison.Ordinal)); } }
        /// <summary>
        /// Returns the full path of the song on the drive.
        /// </summary>
        public string Path
        {
            get { return SongPath.FullPath; }
        }
        /// <summary>
        /// Returns the filename of the song
        /// </summary>
        public string Name
        {
            get { return SongPath.Name; }
        }
        /// <summary>
        /// Used to get the string path value. This is a carryover from an old implementation and will likely be deleted soon.
        /// </summary>
        /// <returns>SongPath.FullPath</returns>
        public string GetPath()
        {
            return Path;
        }
        #endregion
        
        #region System Overrides
        /// <summary>
        /// Compares two local songs based on whether they refer to the same file.
        /// </summary>
        /// <param name="other">Another object to check</param>
        /// <returns>True if other is a local song referring to the same file, false otherwise.</returns>
        public override bool Equals(object other)
        {
            var o = other as LocalSong;
            return o != null && Path.Equals(o.Path) && Id.Equals(o.Id);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}-{1}-{2}-{3}", Path, Title, Artist, Album).GetHashCode();
        }

        public override string ToString()
        {
            return SongPath.Name;
        }
        #endregion
    }
}
