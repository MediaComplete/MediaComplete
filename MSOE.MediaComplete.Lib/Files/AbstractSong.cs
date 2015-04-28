using System;
using System.Web.UI.WebControls;
using MSOE.MediaComplete.Lib.Metadata;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Base class for Songs in the library. Subclasses provide implementations for local files and remotely streamed music
    /// </summary>
    public abstract class AbstractSong 
    {


        public SongPath SongPath { get; set; }
        public abstract string Id { get; }
        public string Title { get; set; }
        public string SupportingArtists { get; set; }
        public string TrackNumber { get; set; }
        public string Year { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public int? Duration { get; set; }
        public string FileType { get { return Path.Substring(Path.LastIndexOf(".", StringComparison.Ordinal)); } }

        public string Path
        {
            get { return SongPath.FullPath; }
        }

        public string Name
        {
            get
            {
                return string.Format("{0} {1} - {2}", TrackNumber, Title, Artist);
            }
        }

        public abstract string GetPath();
        /// <summary>
        /// Creates and returns a subclass of the appropriate type, given the location format in the MediaItem. 
        /// </summary>
        /// <param name="mediaItem">The MediaItem to base the Song off of</param>
        /// <returns>A new Song</returns>
        /// <exception cref="InvalidCastException">Thrown if the matched type of song doesn't provide the expected constructor signature.</exception>
        /// <exception cref="FormatException">Thrown if the MediaItem cannot be recognized as any type.</exception>

        /// <summary>
        /// Enforces an equality check on all subsongs.
        /// </summary>
        /// <param name="other">Another object to compare to</param>
        /// <returns>True if this is logically equivalent to other, false otherwise</returns>
        public new abstract bool Equals(object other);


        public string GetAttribute(MetaAttribute attribute)
        {
            switch (attribute)
            {
                case MetaAttribute.Album:
                    return Album;
                case MetaAttribute.Artist:
                    return Artist;
                case MetaAttribute.Genre:
                    return Genre;
                case MetaAttribute.TrackNumber:
                    return TrackNumber;
                case MetaAttribute.SongTitle:
                    return Title;
                case MetaAttribute.SupportingArtist:
                    return SupportingArtists;
                case MetaAttribute.Year:
                    return Year;
                default:
                    return null;
            }
        }
    }
}