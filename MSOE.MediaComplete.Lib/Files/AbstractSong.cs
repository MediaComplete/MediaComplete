using System;
using MSOE.MediaComplete.Lib.Metadata;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Base class for Songs in the library. Subclasses provide implementations for local files and remotely streamed music
    /// </summary>
    public abstract class AbstractSong 
    {

        /// <summary>
        /// Unique key value used to look up the song in the FileManager
        /// </summary>
        public abstract string Id { get; }

        //All MetaData Attributes are read from TagLib Files.
        #region MetaData Attributes
        /// <summary>
        /// Song Title Property
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Supporting Artists Property
        /// </summary>
        public string SupportingArtists { get; set; }
        /// <summary>
        /// Track Number Property
        /// </summary>
        public string TrackNumber { get; set; }
        /// <summary>
        /// Year Property
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// Album Property
        /// </summary>
        public string Album { get; set; }
        /// <summary>
        /// Artist Property
        /// </summary>
        public string Artist { get; set; }
        /// <summary>
        /// Genre Property
        /// </summary>
        public string Genre { get; set; }
        /// <summary>
        /// Nullable int value representation total duration of song (in seconds)
        /// </summary>
        public int? Duration { get; set; }
        /// <summary>
        /// This function returns a property based on the metaattribute enum. This is used where 
        /// multiple, dynamically determined attributes need to be returned. Otherwise, specific property calls can be made.
        /// </summary>
        /// <param name="attribute">MetaAttribute enum used to get the specific MetaData property from the object</param>
        /// <returns>string value of the appropriate Attribute, or null</returns>
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
        #endregion

        /// <summary>
        /// Enforces an equality check on all subsongs.
        /// </summary>
        /// <param name="other">Another object to compare to</param>
        /// <returns>True if this is logically equivalent to other, false otherwise</returns>
        public new abstract bool Equals(object other);


    }
}