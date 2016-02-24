using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;
using MediaComplete.Lib.Library;
using MediaComplete.Lib.Logging;
using MediaComplete.Lib.Library.DataSource;

namespace MediaComplete.Lib.Playlists
{
    /// <summary>
    /// Represents a playlist. Mutations can be made by manipulating the Songs property.
    /// </summary>
    public class Playlist
    {
        private readonly IM3UFile _file;
        private readonly IPlaylistService _service;

        /// <summary>
        /// Gets the songs contained in this playlist
        /// </summary>
        /// <value>
        /// The songs.
        /// </value>
        public List<AbstractSong> Songs { get; private set; }

        /// <summary>
        /// Gets or sets the title of this playlist
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get { return _file.Name; }
            set { _file.Name = value; }
        }

        /// <summary>
        /// Creates a new playlist based on an underlying M3U file.
        /// </summary>
        /// <param name="service">The playlist service to use</param>
        /// <param name="file">The M3U file</param>
        public Playlist(IPlaylistService service, IM3UFile file)
        {
            _service = service;
            _file = file;
            Songs = _file.Files.Select(service.Create).ToList();
        }

        /// <summary>
        /// Saves any changes made to the Songs list.
        /// </summary>
        public void Save()
        {
            _file.Files.Clear();
            foreach (var song in Songs)
            {
                try
                {
                    _file.Files.Add(_service.ToMediaItem(song as LocalSong));
                }
                catch (FileNotFoundException e)
                {
                    Logger.LogException("File attempting to be added playlist could not be found", e);
                }
            }
            _file.Save();
        }

        /// <summary>
        /// Deletes the underlying playlist file.
        /// </summary>
        public void Delete()
        {
            _file.Delete();
        }

        /// <summary>
        /// Override of ToString, returns Title.
        /// </summary>
        /// <returns>The Title of this playlist.</returns>
        public override string ToString()
        {
            return Title;
        }
    }
}