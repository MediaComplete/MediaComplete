﻿using System;
using System.Collections.Generic;
using System.IO;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Compares two file system objects based on their common paths.
    /// </summary>
    public class FileLocationComparator : EqualityComparer<FileSystemInfo>
    {
        /// <summary>
        /// Compares two file system objects based on their hash codes.
        /// </summary>
        /// <param name="x">The first object</param>
        /// <param name="y">The second object</param>
        /// <returns>The result of the comparison</returns>
        public override bool Equals(FileSystemInfo x, FileSystemInfo y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        /// <summary>
        /// Returns the hashcode of the file system object's path, converted to a common format.
        /// </summary>
        /// <param name="obj">The object to hash</param>
        /// <returns>The hash code</returns>
        public override int GetHashCode(FileSystemInfo obj)
        {
            if (obj != null)
            {
                return Sanitize(obj.FullName).GetHashCode();
            }
            throw new ArgumentNullException("obj");
        }

        /// <summary>
        /// Helper method to format a file system path into a common form.
        /// </summary>
        /// <param name="original">The unfiltered string</param>
        /// <returns>A standardized version</returns>
        private static string Sanitize(string original)
        {
            var str = original.ToLower();
            if (str.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
}