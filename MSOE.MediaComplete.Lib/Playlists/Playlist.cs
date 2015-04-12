using System.Collections.Generic;
using System.IO;
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
        private readonly IM3UFile _file;

        public List<AbstractSong> Songs { get; private set; }
        
        public string Title
        {
            get { return _file.Name; }
            set { _file.Name = value; }
        }

        /// <summary>
        /// Creates a new playlist based on an underlying M3U file.
        /// </summary>
        /// <param name="file">The M3U file</param>
        public Playlist(IM3UFile file)
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
            foreach (var song in Songs)
            {
                try
                {
                    _file.Files.Add(PlaylistService.ToMediaItem(FileManager.Instance.GetFileInfo(song.GetPath())));
                }
                catch (FileNotFoundException)
                {
                    // TODO MC-125 log - 
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