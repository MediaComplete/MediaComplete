using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;
using MSOE.MediaComplete.Lib.Metadata;
using TagLib;
using File = System.IO.File;
using TaglibFile = TagLib.File;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Provides controlled access to the file system.
    /// </summary>
    public class FileManager : IFileManager
    {
        /// <summary>
        /// Dictionary of Id, song pairs. songs carry a reference to the ID identifying them
        /// </summary>
        private readonly Dictionary<string, LocalSong> _cachedSongs;

        /// <summary>
        /// Dictionary of id, <see cref="FileInfo"/> pairs.
        /// </summary>
        private readonly Dictionary<string, FileInfo> _cachedFiles;

        /// <summary>
        /// Windows file watcher; monitors the library for changes
        /// </summary>
        private FileSystemWatcher _watcher;
        
        /// <summary>
        /// singleton instance of the <see cref="FileManager"/>
        /// </summary>
        public static IFileManager Instance { get { return _instance ?? (_instance = new FileManager()); } }
        private static FileManager _instance;

        private FileManager()
        {
            _cachedFiles = new Dictionary<string, FileInfo>();
            _cachedSongs = new Dictionary<string, LocalSong>();
            Initialize(SettingWrapper.MusicDir);
        }

        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="musicDir">Source Directory for populating the dictionaries</param>
        public void Initialize(DirectoryPath musicDir)
        {
            _cachedFiles.Clear();
            _cachedSongs.Clear();
            var files = new DirectoryInfo(musicDir.FullPath).GetFiles("*", SearchOption.AllDirectories).GetMusicFiles();
            foreach (var fileInfo in files)
            {
                AddFileToCache(Guid.NewGuid().ToString(), fileInfo);
            }

            if (_watcher != null)
            {
                _watcher.Dispose();
            }
            _watcher = new FileSystemWatcher(SettingWrapper.MusicDir.FullPath)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };
            _watcher.Renamed += RenamedFile;
            _watcher.Changed += ChangedFile;
            _watcher.Created += CreatedFile;
            _watcher.Deleted += DeletedFile;

            _watcher.EnableRaisingEvents = true;
        }

        #region File Operations
        /// <summary>
        /// Copies a file between two specified paths. 
        /// This is currently only used in the Importer
        /// </summary>
        /// <param name="file">Original location of the song, including filename</param>
        /// <param name="newFile">Destination location of the song, including filename</param>
        public void CopyFile(SongPath file, SongPath newFile)
        {
            File.Copy(file.FullPath, newFile.FullPath);
        }

        /// <summary>
        /// Create a folder at a specified location.
        /// Used by Sorter and to initialize music/playlist folders where necessary
        /// </summary>
        /// <param name="directory">Destination location to create the folder, including the folder name</param>
        public void CreateDirectory(DirectoryPath directory)
        {
            Directory.CreateDirectory(directory.FullPath);
        }

        /// <summary>
        /// Verifies if the specified directory exists.
        /// </summary>
        /// <param name="directory">directory location to check</param>
        /// <returns>true if the directory exists</returns>
        /// <returns>false if the directory does not exist</returns>
        public bool DirectoryExists(DirectoryPath directory)
        {
            return Directory.Exists(directory.FullPath);
        }

        // TODO MC-35 keep directories and files that aren't music, so they can be managed in-application
        /// <summary>
        /// Verifies if the specified directory has no child directories or music files.
        /// 
        /// For now, we don't care about non-music files.
        /// </summary>
        /// <param name="directory">directory location to check</param>
        /// <returns>true if the directory is empty</returns>
        /// <returns>false if the directory contains additional directories or files</returns>
        public bool DirectoryEmpty(DirectoryPath directory)
        {
            var hasDirs = Directory.EnumerateDirectories(directory.FullPath).Any();
            var hasMusic = new DirectoryInfo(directory.FullPath).EnumerateFiles().GetMusicFiles().Any();
            return hasDirs || hasMusic;
        }

        /// <summary>
        /// Moves a file from an old location to a new one
        /// Move operation is performed on the stored FileInfo, and the 
        /// stored song's Path object is updated as well.
        /// </summary>
        /// <param name="oldFile">song that needs to be moved</param>
        /// <param name="newFile">expected location of the file.</param>
        /// <throws>ArgumentException if file does not exist in cache</throws>
        public void MoveFile(LocalSong oldFile, SongPath newFile)
        {
            var sourceDir = oldFile.SongPath.Directory;
            if (!_cachedFiles.ContainsKey(oldFile.Id)) throw new ArgumentException();
            
            _cachedFiles[oldFile.Id].MoveTo(newFile.FullPath);
            ScrubEmptyDirectories(sourceDir);
        }

        /// <summary>
        /// Verifies if the specified file exists.
        /// </summary>
        /// <param name="file">file location to check</param>
        /// <returns>true if the file exists</returns>
        /// <returns>false if the file does not exist</returns>
        public bool FileExists(SongPath file)
        {
            return File.Exists(file.FullPath);
        }

        /// <summary>
        /// used to migrate an entire directories files and folders to a new location.
        /// </summary>
        /// <param name="oldPath">Original directory to move</param>
        /// <param name="newPath">Destination to move directory to</param>
        public void MoveDirectory(DirectoryPath oldPath, DirectoryPath newPath)
        {
            if (Directory.Exists(newPath.FullPath)) throw new IOException("Destination directory already exists");
            var sourceDir = new DirectoryInfo(oldPath.FullPath);

            var folders = sourceDir.GetDirectories().ToList();
            var files = sourceDir.GetFiles().ToList();

            if (!Directory.Exists(newPath.FullPath))
                Directory.CreateDirectory(newPath.FullPath);

            folders.ForEach(x => x.MoveTo(newPath.FullPath + Path.DirectorySeparatorChar + x.Name));
            files.ForEach(x => x.MoveTo(newPath.FullPath + Path.DirectorySeparatorChar + x.Name));

            ScrubEmptyDirectories(sourceDir);
        }


        /// <summary>
        /// Moves a file from the directory of songPath to the directory at newFile. 
        /// This is used in the importer, to move a file that does not exist in our directory into the working directory.
        /// </summary>
        /// <param name="songPath">Where to add the file from</param>
        /// <param name="newFile">The new file name</param>
        public void AddFile(SongPath songPath, SongPath newFile)
        {
            File.Move(songPath.FullPath, newFile.FullPath);
        }

        private static void ScrubEmptyDirectories(DirectoryPath directory)
        {
            var rootInfo = new DirectoryInfo(directory.FullPath);
            foreach (var child in rootInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                ScrubEmptyDirectories(child);
                if (!child.EnumerateFileSystemInfos().Any())
                {
                    child.Delete();
                }
            }
        }
        private static void ScrubEmptyDirectories(DirectoryInfo directory)
        {
            foreach (var child in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                ScrubEmptyDirectories(child);
                if (!child.EnumerateFileSystemInfos().Any())
                {
                    child.Delete();
                }
            }
        }
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
            var sourceDir = deletedSong.SongPath.Directory;
            _cachedSongs.Remove(deletedSong.Id);
            if (deletedSong.Path.Equals(_cachedFiles[deletedSong.Id].FullName) && File.Exists(deletedSong.Path))
                File.Delete(deletedSong.Path);
            _cachedFiles.Remove(deletedSong.Id);
            ScrubEmptyDirectories(sourceDir);
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
        /// Returns a <see cref="LocalSong"/> that has a path that matches the <see cref="MediaItem"/>'s location
        /// </summary>
        /// <param name="mediaItem"><see cref="MediaItem"/> for which the song is needed</param>
        /// <returns><see cref="LocalSong"/> if it exists, null if it doesn't</returns>
        public AbstractSong GetSong(MediaItem mediaItem)
        {
            return _cachedSongs.Values.FirstOrDefault(x => x.SongPath != null && x.Path.Equals(mediaItem.Location));
        }
        #endregion

        #region Data Helpers
        /// <summary>
        /// Helper function for adding new songs to the dictionaries
        /// </summary>
        /// <param name="id">unique ID of the song to be saved</param>
        /// <param name="path">location of the file</param>
        private void AddFileToCache(string id, string path)
        {
            AddFileToCache(id, new FileInfo(path));
        }

        /// <summary>
        /// Helper function for adding new songs to the dictionaries
        /// </summary>
        /// <param name="id">unique ID of the song to be saved</param>
        /// <param name="file">The <see cref="FileInfo"/> object of the file to be saved</param>
        private void AddFileToCache(string id, FileInfo file)
        {
            var newFile = GetNewLocalSong(id, file);
            _cachedSongs.Add(id, newFile);
            _cachedFiles.Add(id, file);
        }

        /// <summary>
        /// Initializes a new LocalSong object, using the path and unique ID provided.
        /// A TagLib file is created, and the necessary fields are read in, in order to populate
        /// the new LocalSong object.
        /// </summary>
        /// <param name="id">unique ID of the file to be saved</param>
        /// <param name="file"><see cref="FileInfo"/> object that needs to be saved</param>
        /// <returns></returns>
        private static LocalSong GetNewLocalSong(string id, FileSystemInfo file)
        {
            return GetNewLocalSong(id, file.FullName);
        }

        /// <summary>
        /// Initializes a new LocalSong object, using the path and unique ID provided.
        /// A TagLib file is created, and the necessary fields are read in, in order to populate
        /// the new LocalSong object.
        /// </summary>
        /// <param name="id">unique ID for the new song</param>
        /// <param name="path">location of the new song</param>
        /// <returns>newly initialized and populated LocalSong object</returns>
        private static LocalSong GetNewLocalSong(string id, string path)
        {
            try
            {
                var tagFile = TaglibFile.Create(path);
                var tag = tagFile.Tag;
                return new LocalSong(id, new SongPath(path))
                {
                    Title = tag.Title,
                    Artists = tag.AlbumArtists,
                    Album = tag.Album,
                    Genres = tag.Genres,
                    Year = tag.Year,
                    TrackNumber = tag.Track,
                    SupportingArtists = tag.Performers,
                    Duration = (int?)tagFile.Properties.Duration.TotalSeconds
                };
            }
            catch (CorruptFileException)
            {
                return new LocalSong(id, new SongPath(path));
            }

        }

        /// <summary>
        /// Helper function to determine whether a fileInfo is currently being used. This function 
        /// is used to help resolve concurrency issues in the FileSystemWatcher EventHandlers
        /// </summary>
        /// <param name="file">the file that needs to be checked</param>
        /// <returns>true if file is being used</returns>
        /// <returns>false if file is not being used</returns>
        /// This guy figured it out first - http://stackoverflow.com/a/9277499
        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        /// <summary>
        /// Used to update the attributes of the TagLibFile associated with the AbstractSong
        /// </summary>
        /// <param name="song"></param>
        private void UpdateFile(LocalSong song)
        {
            var tagFile = TaglibFile.Create(song.Path);

            var tag = tagFile.Tag;
            _cachedSongs[song.Id].Title = tag.Title;
            _cachedSongs[song.Id].Artists = tag.AlbumArtists;
            _cachedSongs[song.Id].Album = tag.Album;
            _cachedSongs[song.Id].Genres = tag.Genres;
            _cachedSongs[song.Id].Year = tag.Year;
            _cachedSongs[song.Id].TrackNumber = tag.Track;
            _cachedSongs[song.Id].SupportingArtists = tag.Performers;
            // Duration is assumed to be fixed
        }
        #endregion

        #region FileWatcher and Events
        /// <summary>
        /// Updates cached song as a result of a Rename event triggered by the system file watcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RenamedFile(object sender, RenamedEventArgs e)
        {
            var retEnum = new List<Tuple<LocalSong, LocalSong>>();
            if (Directory.Exists(e.FullPath))
            {
                var allSongs = _cachedSongs.Values.Where(x => x.Path.StartsWith(e.OldFullPath, StringComparison.Ordinal));
                foreach (var song in allSongs)
                {
                    var oldSong = song;
                    var tempPath = e.FullPath.EndsWith(Path.DirectorySeparatorChar + "", StringComparison.Ordinal)
                        ? e.FullPath
                        : e.FullPath + Path.DirectorySeparatorChar;
                    var newPath = tempPath + song.Name;
                    song.SongPath = new SongPath(newPath);
                    retEnum.Add(new Tuple<LocalSong, LocalSong>(oldSong, song));
                }
            }
            else if (File.Exists(e.FullPath))
            {
                var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.Path.Equals(e.OldFullPath));
                if (firstOrDefault != null)
                {
                    var key = firstOrDefault.Id;

                    _cachedSongs[key].SongPath = new SongPath(e.FullPath);
                    _cachedFiles[key] = new FileInfo(e.FullPath);
                }
            }
            SongRenamed(retEnum);
        }

        /// <summary>
        /// Updates cached song as a result of a 'changed' event being triggered by the system file watcher.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The details from the file system</param>
        public void ChangedFile(object sender, FileSystemEventArgs e)
        {
            var retEnum = new List<LocalSong>();
            var list = _cachedSongs.Values.ToList();
            if (Directory.Exists(e.FullPath))
            {
                try
                {
                    var files =
                        new DirectoryInfo(e.FullPath).EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles();
                    foreach (var file in files)
                    {
                        var newValue = list.FirstOrDefault(x => x != null && x.Path.Equals(file.FullName));
                        if (newValue != null && file.Exists)
                        {
                            newValue.SongPath = new SongPath(file.FullName);
                            UpdateFile(newValue);
                            _cachedFiles[newValue.Id] = file;
                            retEnum.Add(newValue);
                        }
                        else
                        {
                            if (!IsFileLocked(file))
                            {
                                var key = Guid.NewGuid().ToString();
                                AddFileToCache(key, file.FullName);
                                retEnum.Add(_cachedSongs[key]);
                            }
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // This can happen if a directory is deleted. 
                    // It will trigger a delete event AND a changed event, and this will cause an exception.
                }
            }
            else if (File.Exists(e.FullPath))
            {
                var firstOrDefault = list.FirstOrDefault(x => x.Path.Equals(e.FullPath));
                if (firstOrDefault != null)
                {
                    var key = firstOrDefault.Id;
                    UpdateFile(_cachedSongs[key]);
                    retEnum.Add(_cachedSongs[key]);
                }
            }
            SongChanged(retEnum);
        }

        /// <summary>
        /// Updates cached song as a result of a 'deleted' event being triggered by the system file watcher.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The details from the file system</param>
        public void DeletedFile(object sender, FileSystemEventArgs e)
        {
            var retEnum = new List<LocalSong>();
            var list = _cachedSongs.Values.ToList();
            var firstOrDefault = list.FirstOrDefault(x => x!=null && x.Path.Equals(e.FullPath));
            if (firstOrDefault != null)
            {
                var key = firstOrDefault.Id;
                retEnum.Add(_cachedSongs[key]);
                DeleteSong(_cachedSongs[key]);
            }
            else
            {
                var keys = list.Where(x => x!=null && x.Path.StartsWith(e.FullPath, StringComparison.Ordinal)).Select(x => x.Id).ToList();
                foreach (var key in keys)
                {
                    retEnum.Add(_cachedSongs[key]);
                    DeleteSong(_cachedSongs[key]);
                }
            }
            SongDeleted(retEnum);

        }

        /// <summary>
        /// Updates cached song as a result of a 'created' event being triggered by the system file watcher.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The details from the file system</param>
        public void CreatedFile(object sender, FileSystemEventArgs e)
        {
            var retEnum = new List<LocalSong>();
            if (File.Exists(e.FullPath))
            {
                if (_cachedSongs.Values.Any(x => x.SongPath.FullPath.Equals(e.FullPath))) return;
                var key = Guid.NewGuid().ToString();
                AddFileToCache(key, e.FullPath);
                retEnum.Add(_cachedSongs[key]);
            }
            else if (Directory.Exists(e.FullPath))
            {
                var files = new DirectoryInfo(e.FullPath).EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles();
                foreach (var file in files)
                {
                    if (_cachedFiles.Values.Any(x => x.FullName.Equals(file.FullName))) continue;
                    var key = Guid.NewGuid().ToString();
                    AddFileToCache(key, file);
                    retEnum.Add(_cachedSongs[key]);
                }
            }
            SongCreated(retEnum);
        }

        /// <summary>
        /// Occurs when a song is renamed or moved
        /// </summary>
        public event SongRenamedHandler SongRenamed = delegate { };
        /// <summary>
        /// Occurs when a song is modified
        /// </summary>
        public event SongUpdatedHandler SongChanged = delegate { };
        /// <summary>
        /// Occurs when a song is created
        /// </summary>
        public event SongUpdatedHandler SongCreated = delegate { };
        /// <summary>
        /// Occurs whenever a song is deleted
        /// </summary>
        public event SongUpdatedHandler SongDeleted = delegate { };
        #endregion
    }

    /// <summary>
    /// Service for governing access to the file system.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="directory">Source Directory for populating the dictionaries</param>
        void Initialize(DirectoryPath directory);
        /// <summary>
        /// used to migrate an entire directories files and folders to a new location.
        /// </summary>
        /// <param name="oldPath">Original directory to move</param>
        /// <param name="newPath">Destination to move directory to</param>
        void MoveDirectory(DirectoryPath oldPath, DirectoryPath newPath);
        /// <summary>
        /// Moves a file from an old location to a new one
        /// Move operation is performed on the stored FileInfo, and the 
        /// stored song's Path object is updated as well.
        /// </summary>
        /// <param name="oldFile">song that needs to be moved</param>
        /// <param name="newFile">expected location of the file.</param>
        /// <throws>ArgumentException if file does not exist in cache</throws>
        void MoveFile(LocalSong oldFile, SongPath newFile);
        /// <summary>
        /// Copies a file between two specified paths. 
        /// This is currently only used in the Importer
        /// </summary>
        /// <param name="file">Original location of the song, including filename</param>
        /// <param name="newFile">Destination location of the song, including filename</param>
        void CopyFile(SongPath file, SongPath newFile);
        /// <summary>
        /// Verifies if the specified file exists.
        /// </summary>
        /// <param name="file">file location to check</param>
        /// <returns>true if the file exists</returns>
        /// <returns>false if the file does not exist</returns>
        bool FileExists(SongPath file);
        /// <summary>
        /// Verifies if the specified directory exists.
        /// </summary>
        /// <param name="directory">directory location to check</param>
        /// <returns>true if the directory exists</returns>
        /// <returns>false if the directory does not exist</returns>
        bool DirectoryExists(DirectoryPath directory);
        /// <summary>
        /// Verifies if the specified directory has no children.
        /// </summary>
        /// <param name="directory">directory location to check</param>
        /// <returns>true if the directory is empty</returns>
        /// <returns>false if the directory contains additional directories or files</returns>
        bool DirectoryEmpty(DirectoryPath directory);
        /// <summary>
        /// Create a folder at a specified location.
        /// Used by Sorter and to initialize music/playlist folders where necessary
        /// </summary>
        /// <param name="directory">Destination location to create the folder, including the folder name</param>
        void CreateDirectory(DirectoryPath directory);
        /// <summary>
        /// Writes the attributes of the song parameter to the TagLib File and updates the stored FileInfo and song
        /// </summary>
        /// <param name="song">file with updated metadata</param>
        void SaveSong(LocalSong song);
        /// <summary>
        /// Moves a file from the directory of songPath to the directory at newFile. 
        /// This is used in the importer, to move a file that does not exist in our directory into the working directory.
        /// </summary>
        /// <param name="songPath"></param>
        /// <param name="newFile"></param>
        void AddFile(SongPath songPath, SongPath newFile);
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

        /// <summary>
        /// Occurs when a song is renamed or moved
        /// </summary>
        event SongRenamedHandler SongRenamed;

        /// <summary>
        /// Occurs when a song is modified
        /// </summary>
        event SongUpdatedHandler SongChanged;

        /// <summary>
        /// Occurs when a song is created
        /// </summary>
        event SongUpdatedHandler SongCreated;

        /// <summary>
        /// Occurs whenever a song is deleted
        /// </summary>
        event SongUpdatedHandler SongDeleted;
    }

    /// <summary>
    /// Delegate definition for handling changes to song files
    /// </summary>
    /// <param name="songs">The updated songs.</param>
    public delegate void SongUpdatedHandler(IEnumerable<LocalSong> songs);

    /// <summary>
    /// Delegate definition for handling songs that have been moved/renamed
    /// </summary>
    /// <param name="songs">The moved/renamed songs.</param>
    public delegate void SongRenamedHandler(IEnumerable<Tuple<LocalSong, LocalSong>> songs);
}
