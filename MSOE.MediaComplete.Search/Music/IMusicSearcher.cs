using MSOE.MediaComplete.Lib.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Search.Music
{
    /// <summary>
    /// Service for managing a music search index.
    /// </summary>
    interface IMusicIndex
    {
        /// <summary>
        /// Searches for music based on its attributes. All parameters are optional.
        /// 
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
        IEnumerable<AbstractSong> Search(string title = null, 
                                         string album = null, 
                                         string artist = null, 
                                         string supportArtist = null, 
                                         uint? year = null, 
                                         string genre = null);
        
        /// <summary>
        /// Adds or updates songs in the index, translating them out of their metadata fields
        /// </summary>
        /// <param name="songs">The songs to add or update</param>
        void AddOrUpdate(params AbstractSong[] songs);

        /// <summary>
        /// Removes the specified songs from the search index, by ID.
        /// </summary>
        /// <param name="songs">The songs.</param>
        void Remove(params AbstractSong[] songs);
        
        /// <summary>
        /// Clears all songs from the search index - useful for rebuilding.
        /// </summary>
        void Clear();
    }
}
