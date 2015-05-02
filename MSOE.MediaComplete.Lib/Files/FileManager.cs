using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;
using MSOE.MediaComplete.Lib.Metadata;
using File = System.IO.File;
using TaglibFile = TagLib.File;

namespace MSOE.MediaComplete.Lib.Files
{
    public class FileManager : IFileManager
    {
        /// <summary>
        /// Dictionary of Id, song pairs. songs carry a reference to the ID identifying them
        /// </summary>
        private readonly Dictionary<string, LocalSong> _cachedSongs;
        
        /// <summary>
        /// Dictionary of id, fileinfo pairs.
        /// </summary>
        private readonly Dictionary<string, FileInfo> _cachedFiles;
        
        /// <summary>
        /// singleton instance of the Filemanager
        /// </summary>
        private static FileManager _instance;
        
        public static IFileManager Instance { get { return _instance ?? (_instance = new FileManager()); } }

        private FileManager()
        {
            _cachedFiles = new Dictionary<string, FileInfo>();
            _cachedSongs = new Dictionary<string, LocalSong>();
            Initialize(SettingWrapper.MusicDir);
        }
        
        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="directory">Source Directory for populating the dictionarires</param>
        public void Initialize(DirectoryPath directory)
        {
            _cachedFiles.Clear();
            _cachedSongs.Clear();
            var files = new DirectoryInfo(directory.FullPath).GetFiles("*", SearchOption.AllDirectories).GetMusicFiles();
            foreach (var fileInfo in files)
            {
                try
                {
                    AddFileToCache(Guid.NewGuid().ToString(), fileInfo);
                }
                catch (Exception)
                {
                    var song = new LocalSong(Guid.NewGuid().ToString(), new SongPath(fileInfo.FullName));
                    _cachedSongs.Add(song.Id, song);
                    _cachedFiles.Add(song.Id, fileInfo);
                }
            }
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
        /// <param name="directory">Destination location to create the folder, including foldername</param>
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
            if (!_cachedFiles.ContainsKey(oldFile.Id)) throw new ArgumentException();
            _cachedFiles[oldFile.Id].MoveTo(newFile.FullPath);
            _cachedSongs[oldFile.Id].SongPath = newFile;
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

            if (sourceDir.GetDirectories().Length == 0 && sourceDir.GetFiles().Length == 0) sourceDir.Delete();
        }

        /// <summary>
        /// DO NOT USE OFTEN. FILE OPERATIONS SHOULD BE SPECIFIED WITHIN THE FILEMANAGER
        /// Returns a fileinfo object for the specified song. 
        /// This is used in order to get a file object to make sound from
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public FileInfo GetFileInfo(LocalSong song)
        {
            return _cachedFiles[song.Id];
        }
        private readonly List<MetaAttribute> _enumVals = new List<MetaAttribute>
            {
                MetaAttribute.Album, 
                MetaAttribute.Artist, 
                MetaAttribute.Year, 
                MetaAttribute.SupportingArtist, 
                MetaAttribute.TrackNumber, 
                MetaAttribute.SongTitle, 
                MetaAttribute.Genre
            };
        /// <summary>
        /// Writes the attributes of the song parameter to the TagLib File and updates the stored FileInfo and song
        /// </summary>
        /// <param name="song">file with updated metadata</param>
        public void SaveSong(LocalSong song)
        {
            if (!_cachedSongs.ContainsKey(song.Id)) throw new ArgumentException("Song does not exist in cache","song");
            var file = TagLib.File.Create(song.Path);
            
            foreach (var attribute in _enumVals.Where(x => file.GetAttribute(x) == null || !file.GetAttribute(x).Equals(song.GetAttribute(x))))
            {
                file.SetAttribute(attribute, song.GetAttribute(attribute));
            }
            _cachedFiles[song.Id] = new FileInfo(song.Path);
            _cachedSongs[song.Id] = song;
        }

        /// <summary>
        /// Moves a file from the directory of songPath to the directory at newFile. 
        /// This is used in the importer, to move a file that does not exist in our directory into the working directory.
        /// </summary>
        /// <param name="songPath"></param>
        /// <param name="newFile"></param>
        public void AddFile(SongPath songPath, SongPath newFile)
        {
            File.Move(songPath.FullPath, newFile.FullPath);
        }
        #endregion
        
        #region Data Operations
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
            _cachedSongs.Remove(deletedSong.Id);
            _cachedFiles.Remove(deletedSong.Id);
            
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
            return _cachedSongs.Values.FirstOrDefault(x => x.SongPath != null && x.SongPath.FullPath.Equals(mediaItem.Location));
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
        /// <param name="file">The Fileinfo object of the file to be saved</param>
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
        /// <param name="file">fileinfo object that needs to be saved</param>
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
            var tagFile = TaglibFile.Create(path);
            return new LocalSong(id, new SongPath(path))
            {
                Title = tagFile.GetAttribute(MetaAttribute.SongTitle),
                Artist = tagFile.GetAttribute(MetaAttribute.Artist),
                Album = tagFile.GetAttribute(MetaAttribute.Album),
                Genre = tagFile.GetAttribute(MetaAttribute.Genre),
                Year = tagFile.GetAttribute(MetaAttribute.Year),
                TrackNumber = tagFile.GetAttribute(MetaAttribute.TrackNumber),
                SupportingArtists = tagFile.GetAttribute(MetaAttribute.SupportingArtist),
                Duration = (int?) tagFile.Properties.Duration.TotalSeconds
            };
            
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
        #endregion
        
        #region FileWatcher and Events
        /// <summary>
        /// Updates cached song as a result of a Rename event triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RenamedFile(object sender, RenamedEventArgs e)
        {
            var retEnum = new List<Tuple<LocalSong, LocalSong>>();
            var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.SongPath.FullPath.Equals(e.OldFullPath));
            if (firstOrDefault == null)
            {
                var allSongs = _cachedSongs.Values.Where(x => x.SongPath.FullPath.StartsWith(e.OldFullPath, StringComparison.Ordinal));
                foreach (var song in allSongs)
                {
                    var oldSong = song;
                    var tempPath = e.FullPath.EndsWith(Path.DirectorySeparatorChar + "", StringComparison.Ordinal)
                        ? e.FullPath
                        : e.FullPath + Path.DirectorySeparatorChar;
                    var newPath = tempPath+song.Name;
                    song.SongPath = new SongPath(newPath);
                    retEnum.Add(new Tuple<LocalSong, LocalSong>(oldSong, song));
                }
            }
            else 
            {
                var key = firstOrDefault.Id;

                _cachedSongs[key].SongPath = new SongPath(e.FullPath);
                _cachedFiles[key] = new FileInfo(e.FullPath);
            }
            SongRenamed(retEnum);
        }

        /// <summary>
        /// Updates cached song as a result of a 'changed' event being triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChangedFile(object sender, FileSystemEventArgs e)
        {
            var retEnum = new List<LocalSong>();
            if (Directory.Exists(e.FullPath))
            {
                var files = new DirectoryInfo(e.FullPath).EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles();
                foreach (var file in files)
                {
                    var newValue = _cachedSongs.FirstOrDefault(x => x.Value.SongPath.FullPath.Equals(file.FullName)).Value;
                    if (newValue != null)
                    {
                        newValue.SongPath = new SongPath(file.FullName);

                        var tagFile = TaglibFile.Create(file.FullName);
                        _cachedSongs[newValue.Id].Title = tagFile.GetAttribute(MetaAttribute.SongTitle);
                        _cachedSongs[newValue.Id].Artist = tagFile.GetAttribute(MetaAttribute.Artist);
                        _cachedSongs[newValue.Id].Album = tagFile.GetAttribute(MetaAttribute.Album);
                        _cachedSongs[newValue.Id].Genre = tagFile.GetAttribute(MetaAttribute.Genre);
                        _cachedSongs[newValue.Id].Year = tagFile.GetAttribute(MetaAttribute.Year);
                        _cachedSongs[newValue.Id].TrackNumber = tagFile.GetAttribute(MetaAttribute.TrackNumber);
                        _cachedSongs[newValue.Id].SupportingArtists = tagFile.GetAttribute(MetaAttribute.SupportingArtist);
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
            else if(File.Exists(e.FullPath))
            {
                var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.SongPath.FullPath.Equals(e.FullPath));
                if (firstOrDefault != null)
                {
                    var key = firstOrDefault.Id;

                    var tagFile = TaglibFile.Create(e.FullPath);
                    _cachedSongs[key].Title = tagFile.GetAttribute(MetaAttribute.SongTitle);
                    _cachedSongs[key].Artist = tagFile.GetAttribute(MetaAttribute.Artist);
                    _cachedSongs[key].Album = tagFile.GetAttribute(MetaAttribute.Album);
                    _cachedSongs[key].Genre = tagFile.GetAttribute(MetaAttribute.Genre);
                    _cachedSongs[key].Year = tagFile.GetAttribute(MetaAttribute.Year);
                    _cachedSongs[key].TrackNumber = tagFile.GetAttribute(MetaAttribute.TrackNumber);
                    _cachedSongs[key].SupportingArtists = tagFile.GetAttribute(MetaAttribute.SupportingArtist);
                    retEnum.Add(_cachedSongs[key]);
                }
            }
            SongChanged(retEnum);
        }

        /// <summary>
        /// Updates cached song as a result of a 'deleted' event being triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeletedFile(object sender, FileSystemEventArgs e)
        {
            var retEnum = new List<LocalSong>();
            var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.SongPath.FullPath.Equals(e.FullPath));
            if (firstOrDefault == null)
            {
                var keys = _cachedSongs.Values.Where(x => x.SongPath.FullPath.StartsWith(e.FullPath, StringComparison.Ordinal)).Select(x => x.Id).ToList();
                foreach (var key in keys)
                {
                    retEnum.Add(_cachedSongs[key]);
                    DeleteSong(_cachedSongs[key]);
                }
            }
            else
            {
                var key = firstOrDefault.Id;
                retEnum.Add(_cachedSongs[key]);
                DeleteSong(_cachedSongs[key]);
            }
            SongDeleted(retEnum);

        }

        /// <summary>
        /// Updates cached song as a result of a 'created' event being triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CreatedFile(object sender, FileSystemEventArgs e)
        {
            var retEnum = new List<LocalSong>();
            if (File.Exists(e.FullPath))
            {
                var key = Guid.NewGuid().ToString();
                AddFileToCache(key, e.FullPath);
                retEnum.Add(_cachedSongs[key]);
            }
            else if (Directory.Exists(e.FullPath))
            {
                var files = new DirectoryInfo(e.FullPath).EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles();
                foreach (var file in files)
                {
                    var key = Guid.NewGuid().ToString();
                    AddFileToCache(key, file);
                    retEnum.Add(_cachedSongs[key]);
                }
            }
            SongCreated(retEnum);
        }

        
        public event SongRenamedHandler SongRenamed = delegate { };
        public event SongUpdatedHandler SongChanged = delegate { };
        public event SongUpdatedHandler SongCreated = delegate { };
        public event SongUpdatedHandler SongDeleted = delegate { };

        public delegate void SongUpdatedHandler(IEnumerable<LocalSong> songs);
        public delegate void SongRenamedHandler(IEnumerable<Tuple<LocalSong, LocalSong>> songs);
        #endregion

    }
    
    public interface IFileManager
    {
        /// <summary>
        /// Rebuilds the dictionaries using the parameter as the source. 
        /// </summary>
        /// <param name="directory">Source Directory for populating the dictionarires</param>
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
        /// Create a folder at a specified location.
        /// Used by Sorter and to initialize music/playlist folders where necessary
        /// </summary>
        /// <param name="directory">Destination location to create the folder, including foldername</param>
        void CreateDirectory(DirectoryPath directory);
        /// <summary>
        /// DO NOT USE OFTEN. FILE OPERATIONS SHOULD BE SPECIFIED WITHIN THE FILEMANAGER
        /// Returns a fileinfo object for the specified song. 
        /// This is used in order to get a file object to make sound from
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        FileInfo GetFileInfo(LocalSong song);
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
        /// Updates cached song as a result of a Rename event triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RenamedFile(object sender, RenamedEventArgs e);        /// <summary>
        /// Updates cached song as a result of a 'changed' event being triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChangedFile(object sender, FileSystemEventArgs e);
        /// <summary>
        /// Updates cached song as a result of a 'deleted' event being triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeletedFile(object sender, FileSystemEventArgs e);
        /// <summary>
        /// Updates cached song as a result of a 'created' event being triggered by the filewatcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreatedFile(object sender, FileSystemEventArgs e);
        event FileManager.SongRenamedHandler SongRenamed;
        event FileManager.SongUpdatedHandler SongChanged;
        event FileManager.SongUpdatedHandler SongCreated;
        event FileManager.SongUpdatedHandler SongDeleted;
    }
    
}
