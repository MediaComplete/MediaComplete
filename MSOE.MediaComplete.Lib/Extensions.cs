using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
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
            if (dir == null)
            {
                return null;
            }

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
            List<DirectoryInfo> ret;
            if (top.DirectoryEquals(bottom))
            {
                ret = new List<DirectoryInfo> {bottom};
            }
            else
            {
                ret = top.PathSegment(bottom.Parent);
                ret.Add(bottom);
            }
            return ret;
        }

        /// <summary>
        /// Performs an equality test using Windows conventions for directory equality. 
        /// Case insensitive, trailing slashes ignored
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool DirectoryEquals(this DirectoryInfo first, DirectoryInfo second)
        {
            var firstName = first.FullName.TrimEnd(new[] { Path.DirectorySeparatorChar });
            var secondName = second.FullName.TrimEnd(new[] { Path.DirectorySeparatorChar });
            return firstName.Equals(secondName, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Retrieves an ID3 tag value corresponding to the MetaAttribute.
        /// </summary>
        /// <param name="tag">The Tag object derived from a MP3 file</param>
        /// <param name="attr">The MetaAttribute for the specific ID3 value to be returned</param>
        /// <returns>The ID3 value from the Tag</returns>
        // TODO add support for more fields?
        public static string StringForMetaAttribute(this Tag tag, MetaAttribute attr)
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