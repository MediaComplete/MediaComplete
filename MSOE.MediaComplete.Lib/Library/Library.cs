using System.Collections.Generic;
using M3U.NET;
using MediaComplete.Lib.Library.DataSource;
using TaglibFile = TagLib.File;
using System.Threading.Tasks;

namespace MediaComplete.Lib.Library
{
    /// <summary>
    /// The collection of abstractsongs, from every datasource
    /// </summary>
    public class Library : ILibrary
    {
        /// <summary>
        /// singleton instance of the Filemanager
        /// </summary>
        private static Library _instance;
        /// <summary>
        /// The publicly acessible variable for the Library instance
        /// </summary>
        public static ILibrary Instance { get { return _instance ?? (_instance = new Library(FileSystem.Instance)); } }
        private readonly IFileSystem _fileSystem;

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
        public void SaveSong(AbstractSong song)
        {
            var file = song as LocalSong;
            if (file != null)
                _fileSystem.SaveSong(file);
        }

        /// <summary>
        /// Get every song object that exists in the cache
        /// </summary>
        /// <returns>IEnumerable containing every song within the cache</returns>
        public IEnumerable<AbstractSong> GetAllSongs()
        {
            return _fileSystem.GetAllSongs();
        }

        /// <summary>
        /// Removes a song from the caches, AND THE FILESYSTEM AS WELL. THAT FILE IS GONE NOW. DON'T JUST CALL THERE FOR THE HECK OF IT
        /// </summary>
        /// <param name="deletedSong">the song that needs to be deleted</param>
        public void DeleteSong(AbstractSong deletedSong)
        {
            var song = deletedSong as LocalSong;
            if(song != null)
                _fileSystem.DeleteSong(song);
        }

        /// <summary>
        /// Get a LocalSong object with a matching SongPath object
        /// </summary>
        /// <param name="songPath">SongPath object to compare</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        public AbstractSong GetSong(SongPath songPath)
        {
            return _fileSystem.GetSong(songPath);
        }

        /// <summary>
        /// Returns a LocalSong that has a path that matches the MediaItem's location
        /// </summary>
        /// <param name="mediaItem">MediaItem for which the song is needed</param>
        /// <returns>AbstractSong if it exists, null if it doesn't</returns>
        public AbstractSong GetSong(MediaItem mediaItem)
        {
            return _fileSystem.GetSong(mediaItem);
        }
        #endregion
    }

    /// <summary>
    /// interface for the collection of abstractsongs, from every datasource
    /// </summary>
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
        void SaveSong(AbstractSong song);
        /// <summary>
        /// Get every song object that exists in the cache
        /// </summary>
        /// <returns>IEnumerable containing every song within the cache</returns>
        IEnumerable<AbstractSong> GetAllSongs();
        /// <summary>
        /// Removes a song from the caches, AND THE FILESYSTEM AS WELL. THAT FILE IS GONE NOW. DON'T JUST CALL THERE FOR THE HECK OF IT
        /// </summary>
        /// <param name="deletedSong">the song that needs to be deleted</param>
        void DeleteSong(AbstractSong deletedSong);
        /// <summary>
        /// Get a LocalSong object with a matching SongPath object
        /// </summary>
        /// <param name="songPath">SongPath object to compare</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        AbstractSong GetSong(SongPath songPath);
        /// <summary>
        /// Returns a LocalSong that has a path that matches the MediaItem's location
        /// </summary>
        /// <param name="mediaItem">MediaItem for which the song is needed</param>
        /// <returns>LocalSong if it exists, null if it doesn't</returns>
        AbstractSong GetSong(MediaItem mediaItem);
    }

}
