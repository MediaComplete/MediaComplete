using System;
using System.Collections.Generic;
using System.Linq;
using MSOE.MediaComplete.Lib.Metadata;
using TagLib;

namespace MSOE.MediaComplete.Lib.Library
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
        public IEnumerable<string> SupportingArtists { get; set; }
        /// <summary>
        /// Track Number Property
        /// </summary>
        public uint? TrackNumber { get; set; }
        /// <summary>
        /// Year Property
        /// </summary>
        public uint? Year { get; set; }
        /// <summary>
        /// Album Property
        /// </summary>
        public string Album { get; set; }
        /// <summary>
        /// The album art
        /// </summary>
        public IPicture AlbumArt { get; set; }
        /// <summary>
        /// Artist Property
        /// </summary>
        public IEnumerable<string> Artists { get; set; }
        /// <summary>
        /// Genre Property
        /// </summary>
        public IEnumerable<string> Genres { get; set; }
        /// <summary>
        /// Nullable int value representation total duration of song (in seconds)
        /// </summary>
        public int? Duration { get; set; }
        /// <summary>
        /// Nullable int value for the song's rating. Rating is on a scale of 0-255.
        /// </summary>
        public uint? Rating { get; set; }

        #region Special getters and setters

        /// <summary>
        /// This function returns a property based on the meta attribute enumeration. This is used where 
        /// multiple, dynamically determined attributes need to be returned. Otherwise, specific property calls can be made.
        /// </summary>
        /// <param name="attribute">MetaAttribute enumeration used to get the specific MetaData property from the object</param>
        /// <returns>string value of the appropriate Attribute, or null</returns>
        public object GetAttribute(MetaAttribute attribute)
        {
            switch (attribute)
            {
                case MetaAttribute.Album:
                    return Album;
                case MetaAttribute.AlbumArt:
                    return AlbumArt;
                case MetaAttribute.Artist:
                    return Artists;
                case MetaAttribute.Genre:
                    return Genres;
                case MetaAttribute.TrackNumber:
                    return TrackNumber;
                case MetaAttribute.SongTitle:
                    return Title;
                case MetaAttribute.SupportingArtist:
                    return SupportingArtists;
                case MetaAttribute.Year:
                    return Year;
                case MetaAttribute.Rating:
                    return Rating;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieve the specified metadata as a string.
        /// </summary>
        /// <param name="type">The type of metadata</param>
        /// <returns>A string representation of value</returns>
        public string GetAttributeStr(MetaAttribute type)
        {
            switch (type)
            {
                case MetaAttribute.Artist:
                    return Artists.Any() ? Artists.Aggregate((s1, s2) => s1 + "; " + s2) : null;
                case MetaAttribute.SupportingArtist:
                    return SupportingArtists.Any() ? SupportingArtists.Aggregate((s1, s2) => s1 + "; " + s2) : null;
                case MetaAttribute.Genre:
                    return Genres.Any() ? Genres.Aggregate((s1, s2) => s1 + "; " + s2) : null;
                case MetaAttribute.Album:
                    return Album;
                case MetaAttribute.SongTitle:
                    return Title;
                case MetaAttribute.TrackNumber:
                    return TrackNumber.ToString();
                case MetaAttribute.Year:
                    return Year.ToString();
                case MetaAttribute.Rating:
                    return Rating.ToString();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Set the property value from a string.
        /// 
        /// Parameter is assumed to be a semi-colon delimited string of all the artists.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetArtists(string value)
        {
            Artists = value.Split(';').Select(s => s.Trim());
        }

        /// <summary>
        /// Set the property value from a string.
        /// 
        /// Parameter is assumed to be a semi-colon delimited string of all the supporting artists.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetSupportingArtists(string value)
        {
            SupportingArtists = value.Split(';').Select(s => s.Trim());
        }

        /// <summary>
        /// Set the property value from a string.
        /// 
        /// Parameter is assumed to be a semi-colon delimited string of all the genres.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetGenres(string value)
        {
            Genres = value.Split(';').Select(s => s.Trim());
        }

        /// <summary>
        /// Set the property value from a string.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetAlbum(string value)
        {
            Album = value;
        }

        /// <summary>
        /// Set the property value from a string.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetSongTitle(string value)
        {
            Title = value;
        }

        /// <summary>
        /// Set the property value from a string.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <exception cref="FormatException">If value cannot be parsed into a number for numerical metadata</exception>
        /// <exception cref="OverflowException">If value is a negative number or too large</exception>
        public void SetTrackNumber(string value)
        {
            TrackNumber = value == String.Empty ? null : Convert.ToUInt32(value) as uint?;
        }

        /// <summary>
        /// Set the property value from a string.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <exception cref="FormatException">If value cannot be parsed into a number for numerical metadata</exception>
        /// <exception cref="OverflowException">If value is a negative number or too large</exception>
        public void SetYear(string value)
        {
            Year = value == String.Empty ? null : Convert.ToUInt32(value) as uint?;
        }

        /// <summary>
        /// Set the property value from a string.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <exception cref="FormatException">If value cannot be parsed into a number for numerical metadata</exception>
        /// <exception cref="OverflowException">If value is a negative number or too large</exception>
        public void SetRating(string value)
        {
            Rating = value == String.Empty ? null : Convert.ToUInt32(value) as uint?;
        }

        #endregion

        #endregion

        /// <summary>
        /// Enforces an equality check on all songs.
        /// </summary>
        /// <param name="other">Another object to compare to</param>
        /// <returns>True if this is logically equivalent to other, false otherwise</returns>
        public new abstract bool Equals(object other);

        /// <summary>
        /// This is the name of the song
        /// </summary>
        public String Name { get; set; }
    }
}