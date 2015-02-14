using System.Collections.Generic;
using System.Linq;
using M3U.NET;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete.Lib.Playlists
{
    /// <summary>
    /// Represents a playlist. Mutations can be made by manipulating the Songs property.
    /// </summary>
    public class Playlist
    {
        private readonly M3UFile _file;

        public List<AbstractSong> Songs { get; private set; }
        
        public string Name
        {
            get { return _file.Name; }
            set { _file.Name = value; }
        }

        /// <summary>
        /// Creates a new playlist based on an underlying M3U file.
        /// </summary>
        /// <param name="file">The M3U file</param>
        public Playlist(M3UFile file)
        {
            _file = file;
            Songs = _file.Files.Select(AbstractSong.Create).ToList();
        }

        /// <summary>
        /// Saves any changes made to the Songs list.
        /// </summary>
        public void Save()
        {
            _file.Files.Clear(); 
            _file.Files.AddRange(Songs.Select(s => s.ToMediaItem()));
        }

        /// <summary>
        /// Deletes the underlying playlist file.
        /// </summary>
        public void Delete()
        {
            _file.Delete();
        }
    }
}