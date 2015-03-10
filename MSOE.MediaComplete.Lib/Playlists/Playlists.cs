using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;

namespace MSOE.MediaComplete.Lib.Playlists
{
    /// <summary>
    /// Global access class for playlists
    /// </summary>
    public class Playlists
    {
        // TODO MC-192 rewrite class to use mockable injectable File service. Also write tests at this time - see PlaylistsTest.cs

        /// <summary>
        /// The base name for new playlists created without a specified name.
        /// </summary>
        public const string PlaylistDefaultName = "New playlist";

        private static readonly string DefaultExtension = Constants.PlaylistFileExtensions.First();

        /// <summary>
        /// Returns a DirectoryInfo for the playlist storage directory.
        /// </summary>
        /// <returns>A DirectoryInfo for the playlist storage directory</returns>
        public static DirectoryInfo GetPlaylistDir()
        {
            var dir = new DirectoryInfo(SettingWrapper.PlaylistDir);
            if (!dir.Exists)
            {
                Directory.CreateDirectory(dir.FullName);
            }
            return dir;
        }

        /// <summary>
        /// Get all the playlists
        /// </summary>
        /// <returns>An IEnumerable of playlists</returns>
        public static IEnumerable<Playlist> GetAllPlaylists()
        {
            return
                GetPlaylistDir()
                    .EnumerateFiles(Constants.Wildcard, SearchOption.AllDirectories)
                    .Where(f => Constants.PlaylistFileExtensions.Any(e => f.Extension.Equals(e)))
                    .Select(f => new Playlist(new M3UFile(f)));
        }

        /// <summary>
        /// Gets a particular playlist by name
        /// </summary>
        /// <param name="name">The name of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        public static Playlist GetPlaylist(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Playlist name cannot be empty", "name");
            }

            var file = GetPlaylistDir()
                .EnumerateFiles(Constants.Wildcard, SearchOption.AllDirectories)
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name).Equals(name) &&
                                     Constants.PlaylistFileExtensions.Any(e => f.Extension.Equals(e)));

            return file == null ? null : new Playlist(new M3UFile(file));
        }

        /// <summary>
        /// Creates new playlist with the given name.
        /// </summary>
        /// <param name="name">The name of the playlist; will also be the name of the file.</param>
        /// <returns>The new Playlist object</returns>
        /// <exception cref="IOException">If the supplied name is already taken by another playlist file.</exception>
        public static Playlist GetNewPlaylist(string name)
        {
            if (GetPlaylist(name) != null)
            {
                throw new IOException("A playlist by that name already exists.");
            }
            return new Playlist(new M3UFile(new FileInfo(GetPlaylistDir().FullName + Path.DirectorySeparatorChar + name + DefaultExtension)));
        }

        /// <summary>
        /// Creates a new playlist with a default name.
        /// </summary>
        /// <returns>The new Playlist object</returns>
        public static Playlist GetNewPlaylist()
        {
            var name = PlaylistDefaultName;
            var i = 1;
            while (GetPlaylist(name) != null)
            {
                name = String.Format(PlaylistDefaultName + " ({0})", i++);
            }
            return GetNewPlaylist(name);
        }
    }
}