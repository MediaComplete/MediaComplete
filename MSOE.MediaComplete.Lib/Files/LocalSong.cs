
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

        public bool HasParent(DirectoryPath dir)
        {
            return SongPath.HasParent(dir);
        }
        
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

        public override string GetPath()
        {
            return Path; 
        }

    }
}
