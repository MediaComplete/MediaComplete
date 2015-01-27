using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Contains various extension methods for system related calls
    /// </summary>
    public static class SystemExtensions
    {
        /// <summary>
        /// Returns true if the file is located somewhere within the parent's children, recursively.
        /// </summary>
        /// <param name="file">The file in question</param>
        /// <param name="parent">The potential parent we're testing for</param>
        /// <returns></returns>
        public static bool HasParent(this FileInfo file, DirectoryInfo parent)
        {
            var dir = file.Directory;
            while (dir != null && !dir.DirectoryEquals(parent))
            {
                dir = dir.Parent;
            }
            return dir != null;
        }

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
            var firstName = first.FullName.TrimEnd(Path.DirectorySeparatorChar);
            var secondName = second.FullName.TrimEnd(Path.DirectorySeparatorChar);
            return firstName.Equals(secondName, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetValidFileName(this string fileName)
        {
            //special chars not allowed in filename 
            const string specialChars = @"/:*?""<>|#%&.{}~";

            //Replace special chars in raw filename with empty spaces to make it valid  
            Array.ForEach(specialChars.ToCharArray(), specialChar => fileName = fileName.Replace(specialChar.ToString(CultureInfo.CurrentCulture), ""));

            return fileName;

        }

        /// <summary>
        /// Removes empty directories and subdirectories from the given root directory
        /// </summary>
        /// <param name="rootInfo">The root of the tree</param>
        public static void ScrubEmptyDirectories(this DirectoryInfo rootInfo)
        {
            foreach (var child in rootInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                ScrubEmptyDirectories(child);
                if (!child.EnumerateFileSystemInfos().Any())
                {
                    child.Delete();
                }
            }
        }
    }

    public class DirectoryEqualityComparer : IEqualityComparer<DirectoryInfo>
    {
        public bool Equals(DirectoryInfo x, DirectoryInfo y)
        {
            return x.DirectoryEquals(y);
        }

        public int GetHashCode(DirectoryInfo obj)
        {
            return obj.FullName.TrimEnd(Path.DirectorySeparatorChar).ToLower().GetHashCode();
        }
    }
}