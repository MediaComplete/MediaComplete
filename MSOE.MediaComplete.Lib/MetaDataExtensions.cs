using System;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using TagLib;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Contains various extension methods for meta data related operations
    /// </summary>
    internal static class MetaDataExtensions
    {
        /// <summary>
        /// Retrieves an ID3 tag value corresponding to the MetaAttribute.
        /// </summary>
        /// <param name="file">The MP3 File</param>
        /// <param name="attr">The MetaAttribute for the specific ID3 value to be returned</param>
        /// <returns>The ID3 value from the Tag</returns>
        // TODO add support for more fields?
        public static string StringForMetaAttribute(this File file, MetaAttribute attr)
        {
            var tag = file.Tag;
            switch (attr)
            {
                case MetaAttribute.Album:
                    return tag.Album;
                case MetaAttribute.Artist:
                    return tag.FirstAlbumArtist;
                case MetaAttribute.Genre:
                    return tag.FirstGenre;
                case MetaAttribute.Rating:
                    if (tag is TagLib.Id3v2.Tag)
                    {
                        var winId = WindowsIdentity.GetCurrent() ?? WindowsIdentity.GetAnonymous();
                        return
                            RatingFromByte(
                                TagLib.Id3v2.PopularimeterFrame.Get(
                                    tag as TagLib.Id3v2.Tag, winId.Name, true
                                    ).Rating
                                ).ToString(CultureInfo.InvariantCulture);
                    }
                    return RatingFromByte(0).ToString(CultureInfo.InvariantCulture);
                case MetaAttribute.SongTitle:
                    return tag.Title;
                case MetaAttribute.SupportingArtist:
                    return String.Join(",", tag.AlbumArtists.Skip(1));
                case MetaAttribute.TrackNumber:
                    return tag.Track.ToString(CultureInfo.InvariantCulture);
                case MetaAttribute.Year:
                    return tag.Year.ToString(CultureInfo.InvariantCulture);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Translates a byte, representing a song rating from the POPM frame. 
        /// Returns an integer value representing the number of stars. -1 corresponds
        /// to "unrated".
        /// </summary>
        /// <param name="raw">The byte value extracted from the ID3 tag</param>
        /// <returns>An integer representation of the rating</returns>
        private static int RatingFromByte(byte raw)
        {
            //TODO do a modulus instead
            if (raw > 254)
                return 5;
            if (raw > 191)
                return 4;
            if (raw > 127)
                return 3;
            if (raw > 63)
                return 2;
            if (raw > 0)
                return 1;
            return -1; // unrated
        }
    }
}