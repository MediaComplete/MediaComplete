using System.IO;
using M3U.NET;
using MSOE.MediaComplete.Lib.Files;

namespace MSOE.MediaComplete.Lib.Songs
{
    /// <summary>
    /// Implementation of AbstractSong for local files. This is used for MP3 and WMA files.
    /// </summary>
    public class LocalSong : AbstractSong
    {
        public FileInfo File { get; private set; }

        /// <summary>
        /// Constructs a LocalSong from a MediaItem
        /// </summary>
        /// <param name="m">The media item</param>
        public LocalSong (MediaItem m) : this(new FileInfo(m.Location))
        {
        }

        /// <summary>
        /// Constructs a LocalSong from a file
        /// </summary>
        /// <param name="file">The file</param>
        public LocalSong(FileInfo file)
        {
            File = file;
        }
        
        /// <summary>
        /// Compares two local songs based on whether they refer to the same file.
        /// </summary>
        /// <param name="other">Another object to check</param>
        /// <returns>True if other is a local song referring to the same file, false otherwise.</returns>
        public override bool Equals(object other)
        {
            var otherSong = other as LocalSong;
            return otherSong != null && new FileLocationComparator().Equals(File, otherSong.File);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}-{1}-{2}-{3}", Path, Title, Artist, Album).GetHashCode();
        }

        public override string ToString()
        {
            return File.Name;
        }

        public override string GetPath()
        {
            return File.FullName; 
        }
    }
}
