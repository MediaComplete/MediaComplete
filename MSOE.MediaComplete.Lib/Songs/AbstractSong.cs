using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using M3U.NET;

namespace MSOE.MediaComplete.Lib.Songs
{
    /// <summary>
    /// Base class for Songs in the library. Subclasses provide implementations for local files and remotely streamed music
    /// </summary>
    public abstract class AbstractSong
    {
        private static readonly IReadOnlyDictionary<string, Type> TypeDictionary = new Dictionary<string, Type>
        {
            { @"*.\.[" + Constants.MusicFileExtensions.Aggregate((x, y) => x + "|" + y) + "]", typeof (LocalSong) }
            // Future - youtube URLs regex
        };

        /// <summary>
        /// Creates and returns a subclass of the appropriate type, given the location format in the MediaItem. 
        /// </summary>
        /// <param name="mediaItem">The MediaItem to base the Song off of</param>
        /// <returns>A new Song</returns>
        /// <exception cref="InvalidCastException">Thrown if the matched type of song doesn't provide the expected constructor signature.</exception>
        /// <exception cref="FormatException">Thrown if the MediaItem cannot be recognized as any type.</exception>
        public static AbstractSong Create(MediaItem mediaItem)
        {
            foreach (var regex in TypeDictionary)
            {
                var hits = new Regex(regex.Key).Matches(mediaItem.Location).Count;
                if (hits > 0)
                {
                    var constructor = regex.Value.GetConstructor(new[] { typeof(MediaItem) });
                    if (constructor != null)
                        return (AbstractSong)constructor.Invoke(new object[] { mediaItem });
                    throw new InvalidCastException(String.Format("Missing MediaItem constructor for AbstractSong subclass {0}", regex.Value));
                }
            }

            throw new FormatException(String.Format("{0} does not match any known song types", mediaItem.Location));
        }

        /// <summary>
        /// Converts the song to a MediaItem for serialization to a playlist.
        /// </summary>
        /// <returns>A new media item</returns>
        public abstract MediaItem ToMediaItem();
    }
}