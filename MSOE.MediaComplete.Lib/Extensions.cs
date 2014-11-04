using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Contains various extension methods for throughout the project.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Returns the 0-indexed parent directory of a given file. 
        /// </summary>
        /// <param name="file">The invoking file.</param>
        /// <param name="n">The number of directories to go upwards</param>
        /// <returns>The n'th upward directory, where 0 is file's containing directory. If n is greater than the number of parents, it will return the root directory.</returns>
        public static DirectoryInfo Parent(this FileInfo file, int n)
        {
            var dir = file.Directory;
            var i = 0;
            while (i < n && dir.Parent != null)
            {
                dir = dir.Parent;
                i++;
            }
            return dir;
        }

        /// <summary>
        /// Returns a list of directories between the calling object and the specified leaf directory.
        /// </summary>
        /// <param name="top">The calling object</param>
        /// <param name="bottom">The bottom of the path</param>
        /// <returns>A list of directories between top and bottom</returns>
        public static List<DirectoryInfo> PathSegment(this DirectoryInfo top, DirectoryInfo bottom)
        {
            if (top == bottom)
            {
                return new List<DirectoryInfo>() {top};
            }
            else
            {
                var list = top.PathSegment(bottom.Parent);
                list.Add(bottom);
                return list;
            }
        }

        /// <summary>
        /// Retrieves an ID3 tag value corresponding to the MetaAttribute.
        /// </summary>
        /// <param name="tag">The Tag object derived from a MP3 file</param>
        /// <param name="attr">The MetaAttribute for the specific ID3 value to be returned</param>
        /// <returns>The ID3 value from the Tag</returns>
        // TODO this is readonly. 
        // TODO add support for more fields?
        public static IComparable GetComparableForMetaAttribute(this Tag tag, MetaAttribute attr)
        {
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
                        return
                            RatingFromByte(
                                TagLib.Id3v2.PopularimeterFrame.Get(tag as TagLib.Id3v2.Tag, "WindowsUser", true).Rating);
                    else
                        return RatingFromByte(0);
                case MetaAttribute.SongTitle:
                    return tag.Title;
                case MetaAttribute.SupportingArtist:
                    return String.Join(",", tag.AlbumArtists.Skip(1));
                case MetaAttribute.TrackNumber:
                    return tag.Track;
                case MetaAttribute.Year:
                    return tag.Year;
                default:
                    return null;
            }
        }

        // TODO localize
        /// <summary>
        /// Translates a byte, representing a song rating from the POPM frame. 
        /// Returns an integer value representing the number of stars. -1 corresponds
        /// to "unrated".
        /// </summary>
        /// <param name="raw">The byte value extracted from the ID3 tag</param>
        /// <returns>An integer representation of the rating</returns>
        private static int RatingFromByte(byte raw)
        {
            if (raw > 254)
                return 5;
            else if (raw > 191)
                return 4;
            else if (raw > 127)
                return 3;
            else if (raw > 63)
                return 2;
            else if (raw > 0)
                return 1;
            else
                return -1;
        }
    }
}