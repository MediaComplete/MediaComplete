using System;
using System.Collections.Generic;
using MSOE.MediaComplete.Lib.Files;

namespace MSOE.MediaComplete.Search.Music
{
    /// <summary>
    /// Searches for files using the local file system search index.
    /// </summary>
    public class FileMusicSearcher : IMusicIndex
    {
        private IIndex index;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMusicSearcher"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public FileMusicSearcher(IIndex index)
        {
            this.index = index;
        }

        /// <summary>
        /// Adds or updates songs in the index, translating them out of their metadata fields
        /// </summary>
        /// <param name="songs">The songs to add or update</param>
        public void AddOrUpdate(params AbstractSong[] songs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears all songs from the search index - useful for rebuilding.
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the specified songs from the search index, by ID.
        /// </summary>
        /// <param name="songs">The songs.</param>
        public void Remove(params AbstractSong[] songs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for music based on its attributes. All parameters are optional.
        /// Named parameter syntax is recommend for readability, e.g.
        /// <code>index.Search(title: "Never gonna give you up", artist: "Rick Astly")</code>
        /// </summary>
        /// <param name="title">The song title.</param>
        /// <param name="album">The album.</param>
        /// <param name="artist">The artist(s).</param>
        /// <param name="supportArtist">The supporting artist(s).</param>
        /// <param name="year">The album release year.</param>
        /// <param name="genre">The genre.</param>
        /// <returns></returns>
        public IEnumerable<AbstractSong> Search(string title = null, string album = null, string artist = null, string supportArtist = null, uint? year = default(uint?), string genre = null)
        {
            throw new NotImplementedException();
        }
    }
}
