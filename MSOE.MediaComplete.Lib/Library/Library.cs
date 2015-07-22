using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Library.FileSystem;
using TagLib;
using File = System.IO.File;
using TaglibFile = TagLib.File;

namespace MSOE.MediaComplete.Lib.Library
{
    public class Library : ILibrary
    {

        /// <summary>
        /// singleton instance of the Filemanager
        /// </summary>
        private static Library _instance;
        public static ILibrary Instance { get { return _instance ?? (_instance = new Library(FileSystem.FileSystem.Instance)); } }
        private IFileSystem _fileSystem;

        private Library(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            Initialize(SettingWrapper.MusicDir);
        }

        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="musicDir">Source Directory for populating the dictionarires</param>
        public void Initialize(DirectoryPath musicDir)
        {
            _fileSystem.Initialize(musicDir);
        }

        #region File Operations

        #endregion

        #region Data Operations
        /// <summary>
        /// Writes the attributes of the song parameter to the TagLib File and updates the stored FileInfo and song
        /// </summary>
        /// <param name="song">file with updated metadata</param>
        public void SaveSong(LocalSong song)
        {
            if (!_cachedSongs.ContainsKey(song.Id)) throw new ArgumentException("Song does not exist in cache", "song");
            var file = TagLib.File.Create(song.Path);

            foreach (var attribute in Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList()
                .Where(x => file.GetAttribute(x) == null || !file.GetAttribute(x).Equals(song.GetAttribute(x))))
            {
                file.SetAttribute(attribute, song.GetAttribute(attribute));
            }
            try
            {
                file.Save(); //TODO: MC-4 add catch for save when editing a file while it is playing
            }
            catch (UnauthorizedAccessException)
            {
                // TODO MC-125 log
                StatusBarHandler.Instance.ChangeStatusBarMessage("Save-Error", StatusBarHandler.StatusIcon.Error);
            }
            _cachedFiles[song.Id] = new FileInfo(song.Path);
            _cachedSongs[song.Id] = song;
        }

        /// <summary>
        /// Get every song object that exists in the cache
        /// </summary>
        /// <returns>IEnumerable containing every song within the cache</returns>
        public IEnumerable<LocalSong> GetAllSongs()
        {
            return _cachedSongs.Values;
        }

        /// <summary>
        /// Removes a song from the caches, AND THE FILESYSTEM AS WELL. THAT FILE IS GONE NOW. DON'T JUST CALL THERE FOR THE HECK OF IT
        /// </summary>
        /// <param name="deletedSong">the song that needs to be deleted</param>
        public void DeleteSong(LocalSong deletedSong)
        {
            _fileSystem.DeleteFile(deletedSong);
        }

        /// <summary>
        /// Get a LocalSong object with a matching SongPath object
        /// </summary>
        /// <param name="songPath">SongPath object to compare</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        public LocalSong GetSong(SongPath songPath)
        {
            return _cachedSongs.Values.FirstOrDefault(x => x.SongPath.Equals(songPath));
        }

        /// <summary>
        /// Returns a LocalSong that has a path that matches the MediaItem's location
        /// </summary>
        /// <param name="mediaItem">MediaItem for which the song is needed</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        public AbstractSong GetSong(MediaItem mediaItem)
        {
            return _cachedSongs.Values.FirstOrDefault(x => x.SongPath != null && x.Path.Equals(mediaItem.Location));
        }
        #endregion



    }

    public interface ILibrary
    {
        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="directory">Source Directory for populating the dictionarires</param>
        void Initialize(DirectoryPath directory);
        /// <summary>
        /// Writes the attributes of the song parameter to the TagLib File and updates the stored FileInfo and song
        /// </summary>
        /// <param name="song">file with updated metadata</param>
        void SaveSong(LocalSong song);
        /// <summary>
        /// Get every song object that exists in the cache
        /// </summary>
        /// <returns>IEnumerable containing every song within the cache</returns>
        IEnumerable<LocalSong> GetAllSongs();
        /// <summary>
        /// Removes a song from the caches, AND THE FILESYSTEM AS WELL. THAT FILE IS GONE NOW. DON'T JUST CALL THERE FOR THE HECK OF IT
        /// </summary>
        /// <param name="deletedSong">the song that needs to be deleted</param>
        void DeleteSong(LocalSong deletedSong);
        /// <summary>
        /// Get a LocalSong object with a matching SongPath object
        /// </summary>
        /// <param name="songPath">SongPath object to compare</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        LocalSong GetSong(SongPath songPath);
        /// <summary>
        /// Returns a LocalSong that has a path that matches the MediaItem's location
        /// </summary>
        /// <param name="mediaItem">MediaItem for which the song is needed</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        AbstractSong GetSong(MediaItem mediaItem);
    }

}
