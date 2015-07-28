using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using M3U.NET;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Library;

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

        private static IPlaylistService _service = new PlaylistServiceImpl(Dependency.Resolve<ILibrary>());

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

        /// <summary>
        /// Converts a local song to a playlist's media item
        /// </summary>
        /// <param name="song">The song.</param>
        /// <returns>A media item that can be saved in a playlist</returns>
        public static MediaItem ToMediaItem(LocalSong song)
        {
            return _service.ToMediaItem(song);
        }

        /// <summary>
        /// Creates a song object for a playlist's media item
        /// </summary>
        /// <param name="mediaItem">The media item.</param>
        /// <returns>A song</returns>
        public static AbstractSong Create(MediaItem mediaItem)
        {
            return _service.Create(mediaItem);
        }
    }
    #endregion

    #region Interface
    /// <summary>
    /// A service for accessing and manipulating playlists
    /// </summary>
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

        /// <summary>
        /// Creates a song object for a playlist's media item
        /// </summary>
        /// <param name="mediaItem">The media item.</param>
        /// <returns>A song</returns>
        AbstractSong Create(MediaItem mediaItem);

        /// <summary>
        /// Converts a local song to a playlist's media item
        /// </summary>
        /// <param name="song">The song.</param>
        /// <returns>A media item that can be saved in a playlist</returns>
        MediaItem ToMediaItem(LocalSong song);
    }
    #endregion

    #region File implementation
    /// <summary>
    /// Local file implementation of a playlist service
    /// </summary>
    public class PlaylistServiceImpl : IPlaylistService
    {
        private readonly ILibrary _library;
        /// <summary>
        /// public constructor used by the dependency injection class
        /// </summary>
        /// <param name="library"></param>
        public PlaylistServiceImpl(ILibrary library)
        {
            _library = library;
        }

        // TODO rewrite class to use mock-able injectable File service. Also write tests at this time - see PlaylistsTest.cs

        private static readonly string DefaultExtension = Constants.PlaylistFileExtensions.First();

        /// <summary>
        /// Returns a DirectoryInfo for the playlist storage directory.
        /// </summary>
        /// <returns>A DirectoryInfo for the playlist storage directory</returns>
        public DirectoryInfo GetDirectoryInfo()
        {
            var dir = new DirectoryInfo(SettingWrapper.PlaylistDir.FullPath);
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
            var y =
                GetDirectoryInfo()
                    .EnumerateFiles(Constants.Wildcard, SearchOption.AllDirectories)
                    .Where(f => Constants.PlaylistFileExtensions.Any(e => f.Extension.Equals(e)));
            var z = y.Select(f => new Playlist(this, new M3UFile(f)));
            return z;

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

            return file == null ? null : new Playlist(Dependency.Resolve<IPlaylistService>(), new M3UFile(file));
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
            return new Playlist(Dependency.Resolve<IPlaylistService>(), new M3UFile(new FileInfo(GetDirectoryInfo().FullName + Path.DirectorySeparatorChar + title + DefaultExtension)));
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
        /// <summary>
        /// Converts this LocalSong to a MediaItem so it can be serialized to a playlist.
        /// </summary>
        /// <param name="song">The song.</param>
        /// <returns>
        /// A new media item
        /// </returns>
        public MediaItem ToMediaItem(LocalSong song)
        {
            return new MediaItem
            {
                Location = song.Path,
                Inf = song.Artists + " - " + song.Title,
                Runtime = song.Duration ?? 0
            };
        }

        private static readonly IReadOnlyDictionary<string, Type> TypeDictionary = new Dictionary<string, Type>
        {
            // MP3/WMA file regex
            {@".*\.[" + Constants.MusicFileExtensions.Aggregate((x, y) => x + "|" + y) + "]", typeof (LocalSong)}
            // Future - YouTube URLs regex
        };

        /// <summary>
        /// Creates a song object for a playlist's media item
        /// </summary>
        /// <param name="mediaItem">The media item.</param>
        /// <returns>
        /// A song
        /// </returns>
        /// <exception cref="FormatException">Occurs when a media item cannot be handled by this library</exception>
        public AbstractSong Create(MediaItem mediaItem)
        {
            if (TypeDictionary.Select(regex => new Regex(regex.Key).Matches(mediaItem.Location).Count).Any(hits => hits > 0))
            {
                return _library.GetSong(mediaItem) ?? new ErrorSong(null){ Title = mediaItem.Inf };
            }

            throw new FormatException(String.Format("{0} does not match any known song types", mediaItem.Location));
        }
    }
    #endregion
}