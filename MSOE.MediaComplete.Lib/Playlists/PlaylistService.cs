using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;

namespace MSOE.MediaComplete.Lib.Playlists
{
    #region Static Interface
    /// <summary>
    /// Global access class for playlists
    /// </summary>
    public static class PlaylistService
    {
        /// <summary>
        /// The base name for new playlists created without a specified name.
        /// </summary>
        public const string PlaylistDefaultName = "New playlist";

        private static IPlaylistService _service = new PlaylistServiceImpl();

        /// <summary>
        /// Provides a way to substitute the implementation for playlist operations. 
        /// </summary>
        /// <param name="newService">New instance of IPlaylistService</param>
        public static void SetService(IPlaylistService newService)
        {
            _service = newService ?? _service;
        }

        /// <summary>
        /// Returns a DirectoryInfo for the playlist storage directory.
        /// </summary>
        /// <returns>A DirectoryInfo for the playlist storage directory</returns>
        public static DirectoryInfo GetDirectoryInfo()
        {
            return _service.GetDirectoryInfo();
        }

        /// <summary>
        /// Get all the playlists
        /// </summary>
        /// <returns>An IEnumerable of playlists</returns>
        public static IEnumerable<Playlist> GetAllPlaylists()
        {
            return _service.GetAllPlaylists();
        }

        /// <summary>
        /// Gets a particular playlist by name
        /// </summary>
        /// <param name="name">The name of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        public static Playlist GetPlaylist(string name)
        {
            return _service.GetPlaylist(name);
        }

        /// <summary>
        /// Creates new playlist with the given name.
        /// </summary>
        /// <param name="name">The name of the playlist; will also be the name of the file.</param>
        /// <returns>The new Playlist object</returns>
        /// <exception cref="IOException">If the supplied name is already taken by another playlist file.</exception>
        public static Playlist CreatePlaylist(string name)
        {
            return _service.CreatePlaylist(name);
        }

        /// <summary>
        /// Creates a new playlist with a default name.
        /// </summary>
        /// <returns>The new Playlist object</returns>
        public static Playlist CreatePlaylist()
        {
            return _service.CreatePlaylist();
        }
    }
    #endregion

    #region Interface
    public interface IPlaylistService
    {
        /// <summary>
        /// Returns a DirectoryInfo for the playlist storage directory.
        /// </summary>
        /// <returns>A DirectoryInfo for the playlist storage directory</returns>
        DirectoryInfo GetDirectoryInfo();

        /// <summary>
        /// Get all the playlists
        /// </summary>
        /// <returns>An IEnumerable of playlists</returns>
        IEnumerable<Playlist> GetAllPlaylists();

        /// <summary>
        /// Gets a particular playlist by name
        /// </summary>
        /// <param name="name">The name of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        Playlist GetPlaylist(string name);

        /// <summary>
        /// Creates new playlist with the given name.
        /// </summary>
        /// <param name="name">The name of the playlist; will also be the name of the file.</param>
        /// <returns>The new Playlist object</returns>
        /// <exception cref="IOException">If the supplied name is already taken by another playlist file.</exception>
        Playlist CreatePlaylist(string name);

        /// <summary>
        /// Creates a new playlist with a default name.
        /// </summary>
        /// <returns>The new Playlist object</returns>
        Playlist CreatePlaylist();
    }
    #endregion

    #region File implementation
    public class PlaylistServiceImpl : IPlaylistService
    {
        internal PlaylistServiceImpl()
        {
        }

        // TODO MC-192 rewrite class to use mockable injectable File service. Also write tests at this time - see PlaylistsTest.cs

        private static readonly string DefaultExtension = Constants.PlaylistFileExtensions.First();

        /// <summary>
        /// Returns a DirectoryInfo for the playlist storage directory.
        /// </summary>
        /// <returns>A DirectoryInfo for the playlist storage directory</returns>
        public DirectoryInfo GetDirectoryInfo()
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
        public IEnumerable<Playlist> GetAllPlaylists()
        {
            return
                GetDirectoryInfo()
                    .EnumerateFiles(Constants.Wildcard, SearchOption.AllDirectories)
                    .Where(f => Constants.PlaylistFileExtensions.Any(e => f.Extension.Equals(e)))
                    .Select(f => new Playlist(new M3UFile(f)));
        }

        /// <summary>
        /// Gets a particular playlist by name
        /// </summary>
        /// <param name="name">The name of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        public Playlist GetPlaylist(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Playlist name cannot be empty", "name");
            }

            var file = GetDirectoryInfo()
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
        public Playlist CreatePlaylist(string name)
        {
            if (GetPlaylist(name) != null)
            {
                throw new IOException("A playlist by that name already exists.");
            }
            return new Playlist(new M3UFile(new FileInfo(GetDirectoryInfo().FullName + Path.DirectorySeparatorChar + name + DefaultExtension)));
        }

        /// <summary>
        /// Creates a new playlist with a default name.
        /// </summary>
        /// <returns>The new Playlist object</returns>
        public Playlist CreatePlaylist()
        {
            var name = PlaylistService.PlaylistDefaultName;
            var i = 1;
            while (GetPlaylist(name) != null)
            {
                name = String.Format(PlaylistService.PlaylistDefaultName + " ({0})", i++);
            }
            return CreatePlaylist(name);
        }
    }
    #endregion
}