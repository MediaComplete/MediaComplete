using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using TagLib.Id3v2;
using File = TagLib.File;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Contains various extension methods for meta data related operations
    /// </summary>
    internal static class MetaDataExtensions
    {
        public static bool ContainsMusicFile(this DirectoryInfo dir, File matchFile)
        {
            if (dir == null || matchFile == null || !dir.Exists)
            {
                return false;
            }

            return dir.EnumerateFiles(Constants.MusicFilePattern).Any(f =>
            {
                var file = File.Create(f.FullName);
                return matchFile.MusicFileEquals(file);
            });
        }

        /// <summary>
        /// Tests music files for equality. This is done based on the album name and track number, 
        /// since the actual file names are often different. This could still cause issues if one 
        /// file or the other hasn't had its metadata properly filled out, but this is really the best we can do.
        /// </summary>
        /// <param name="file">The first MP3 file</param>
        /// <param name="other">The second MP3 file</param>
        /// <returns>True if the files logically represent the same song.</returns>
        public static bool MusicFileEquals(this File file, File other)
        {
            if (file == null || other == null)
            {
                return false;
            }

            var fileTag = file.Tag;
            var otherTag = other.Tag;
            return fileTag.Album == otherTag.Album && fileTag.Track == otherTag.Track;
        }

        public static void SetMetaAttribute(this File file, MetaAttribute attr, object value)
        {
            if (value == null) return;
            var tag = file.Tag;
            switch (attr)
            {
                case MetaAttribute.Album:
                    tag.Album = (string)value;
                    break;
                case MetaAttribute.Artist:
                    //tag.FirstAlbumArtist = value; //TODO no setter
                    break;
                case MetaAttribute.Genre:
                    //tag.FirstGenre = value; //TODO no setter
                    break;
                case MetaAttribute.Rating:
                    //TODO
                    break;
                case MetaAttribute.SongTitle:
                    tag.Title = (string)value;
                    break;
                case MetaAttribute.SupportingArtist:
                    //tag.AlbumArtists += value; //TODO no setter
                    break;
                case MetaAttribute.TrackNumber:
                    tag.Track = (uint)value;
                    break;
                case MetaAttribute.Year:
                    tag.Year = (uint)value;
                    break;
                default:
                    return;
            }
            file.Save();
        }

        /// <summary>
        /// Retrieves an ID3 tag value corresponding to the MetaAttribute.
        /// </summary>
        /// <param name="file">The MP3 File</param>
        /// <param name="attr">The MetaAttribute for the specific ID3 value to be returned</param>
        /// <returns>The ID3 value from the tag</returns>
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
                    var tag1 = tag as Tag;
                    if (tag1 != null)
                    {
                        var winId = WindowsIdentity.GetCurrent() ?? WindowsIdentity.GetAnonymous();
                        return
                            RatingFromByte(
                                PopularimeterFrame.Get(
                                    tag1, winId.Name, true
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