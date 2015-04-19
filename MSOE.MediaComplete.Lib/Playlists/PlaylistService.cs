﻿using System;
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
        /// The base title for new playlists created without a specified title.
        /// </summary>
        public const string PlaylistDefaultTitle = "New playlist";

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
        /// Gets a particular playlist by title
        /// </summary>
        /// <param name="title">The title of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        public static Playlist GetPlaylist(string title)
        {
            return _service.GetPlaylist(title);
        }

        /// <summary>
        /// Creates new playlist with the given title.
        /// </summary>
        /// <param name="title">The title of the playlist; will also be the name of the file.</param>
        /// <returns>The new Playlist object</returns>
        /// <exception cref="IOException">If the supplied title is already taken by another playlist file.</exception>
        public static Playlist CreatePlaylist(string title)
        {
            return _service.CreatePlaylist(title);
        }

        /// <summary>
        /// Creates a new playlist with a default title.
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
        /// Gets a particular playlist by title
        /// </summary>
        /// <param name="title">The title of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        Playlist GetPlaylist(string title);

        /// <summary>
        /// Creates new playlist with the given title.
        /// </summary>
        /// <param name="title">The title of the playlist; will also be the name of the file.</param>
        /// <returns>The new Playlist object</returns>
        /// <exception cref="IOException">If the supplied title is already taken by another playlist file.</exception>
        Playlist CreatePlaylist(string title);

        /// <summary>
        /// Creates a new playlist with a default title.
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
        /// Gets a particular playlist by title
        /// </summary>
        /// <param name="title">The title of the playlist, which corresponds to the filename. It is case-sensitive.</param>
        /// <returns>The playlist object, or null if it's not found</returns>
        public Playlist GetPlaylist(string title)
        {
            if (title == null)
            {
                throw new ArgumentNullException("title");
            }

            if (String.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Playlist title cannot be empty", "title");
            }

            var file = GetDirectoryInfo()
                .EnumerateFiles(Constants.Wildcard, SearchOption.AllDirectories)
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name).Equals(title) &&
                                     Constants.PlaylistFileExtensions.Any(e => f.Extension.Equals(e)));

            return file == null ? null : new Playlist(new M3UFile(file));
        }

        /// <summary>
        /// Creates new playlist with the given title.
        /// </summary>
        /// <param name="title">The title of the playlist; will also be the name of the file.</param>
        /// <returns>The new Playlist object</returns>
        /// <exception cref="IOException">If the supplied title is already taken by another playlist file.</exception>
        public Playlist CreatePlaylist(string title)
        {
            if (GetPlaylist(title) != null)
            {
                throw new IOException("A playlist by that title already exists.");
            }
            return new Playlist(new M3UFile(new FileInfo(GetDirectoryInfo().FullName + Path.DirectorySeparatorChar + title + DefaultExtension)));
        }

        /// <summary>
        /// Creates a new playlist with a default title.
        /// </summary>
        /// <returns>The new Playlist object</returns>
        public Playlist CreatePlaylist()
        {
            var title = PlaylistService.PlaylistDefaultTitle;
            var i = 1;
            while (GetPlaylist(title) != null)
            {
                title = String.Format(PlaylistService.PlaylistDefaultTitle + " ({0})", i++);
            }
            return CreatePlaylist(title);
        }
    }
    #endregion
}