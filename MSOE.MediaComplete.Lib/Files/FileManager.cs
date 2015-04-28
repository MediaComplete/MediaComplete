using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M3U.NET;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Files;
using TaglibFile = TagLib.File;

namespace MSOE.MediaComplete.Lib.Files
{
    public class FileManager : IFileManager
    {
        private readonly Dictionary<string, LocalSong> _cachedSongs;
        private readonly Dictionary<string, FileInfo> _cachedFiles; 
        public static readonly FileManager Instance = new FileManager();

        private FileManager()
        {
            _cachedFiles = new Dictionary<string, FileInfo>();
            _cachedSongs = new Dictionary<string, LocalSong>();
        }
        
        public void Initialize(DirectoryPath directory)
        {
            var files = new DirectoryInfo(directory.FullPath).GetFiles("*", SearchOption.AllDirectories);
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
        #region Data Operations
        public IEnumerable<LocalSong> GetAllSongs()
        {
            return _cachedSongs.Values;
        }

        public void DeleteSong(LocalSong deletedSong)
        {
            _cachedSongs.Remove(deletedSong.Id);
            _cachedSongs.Remove(deletedSong.Id);
        }
        public LocalSong GetSong(SongPath s)
        {
            return _cachedSongs.Values.First(x => x.SongPath.Equals(s));
        }

        public static IEnumerable<SongPath> GetSongPaths(string selectedDir)
        {
            var d = new DirectoryInfo(selectedDir);
            return d.EnumerateFiles("*", SearchOption.AllDirectories)
                    .GetMusicFiles().Select(x => new SongPath(x.FullName));
        }
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
                Duration = (int?)tagFile.Properties.Duration.TotalSeconds
            };
        }
        private static LocalSong GetNewLocalSong(string id, FileSystemInfo file)
        {
            return GetNewLocalSong(id, file.FullName);
        }
        private void AddFileToCache(string id, string path)
        {
            var newFile = GetNewLocalSong(id, path);
            _cachedSongs.Add(id, newFile);
            _cachedFiles.Add(id, new FileInfo(path));
        }
        private void AddFileToCache(string id, FileInfo file)
        {
            var newFile = GetNewLocalSong(id, file);
            _cachedSongs.Add(id, newFile);
            _cachedFiles.Add(id, file);
        }
        #endregion
        #region File Operations
        public void CopyFile(SongPath file, SongPath newFile)
        {
            File.Copy(file.FullPath, newFile.FullPath);
        }
        
        public void CreateDirectory(DirectoryPath directory)
        {
            Directory.CreateDirectory(directory.FullPath);
        }

        public bool DirectoryExists(DirectoryPath directory)
        {
            return Directory.Exists(directory.FullPath);
        }

        public void MoveFile(SongPath oldFile, SongPath newFile)
        {
            if (!_cachedSongs.ContainsKey(oldFile.FullPath)) throw new ArgumentException();
            File.Move(oldFile.FullPath, newFile.FullPath);
        }

        public bool FileExists(SongPath fileName)
        {
            return File.Exists(fileName.FullPath);
        }

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

        public FileInfo GetFileInfo(LocalSong song)
        {
            return _cachedFiles[song.Id];
        }
        public void SaveSong(AbstractSong song)
        {
            if (song is LocalSong)
            {
                var file = TagLib.File.Create(song.Path);
                file.SetAttribute(MetaAttribute.SongTitle, song.Title);
                file.SetAttribute(MetaAttribute.Album, song.Album);
                file.SetAttribute(MetaAttribute.Artist, song.Title);
                file.SetAttribute(MetaAttribute.SupportingArtist, song.SupportingArtists);
                file.SetAttribute(MetaAttribute.Year, song.Year);
                file.SetAttribute(MetaAttribute.Genre, song.Genre);
                file.SetAttribute(MetaAttribute.TrackNumber, song.TrackNumber);
            }
        }

        #endregion
        #region FileWatcher and Events 
        public void UpdateFile(object sender, RenamedEventArgs e)
        {
            var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.SongPath.Equals(new SongPath(e.OldFullPath)));
            if (firstOrDefault == null)return;
            
            var key = firstOrDefault.Id;
            var song = _cachedSongs[key];

            _cachedSongs[key].SongPath = new SongPath(e.FullPath);
            _cachedFiles[key] = new FileInfo(e.FullPath);

            SongRenamed(song, _cachedSongs[key]);
            
        }

        public void ChangedFile(object sender, FileSystemEventArgs e)
        {
            var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.SongPath.Equals(new SongPath(e.FullPath)));
            if (firstOrDefault == null)
            {
                var key = Guid.NewGuid().ToString();
                AddFileToCache(key, e.FullPath);
                SongCreated(_cachedSongs[key]);
            }
            else { 
                var key = firstOrDefault.Id;

                var tagFile = TaglibFile.Create(e.FullPath);
                _cachedSongs[key].Title = tagFile.GetAttribute(MetaAttribute.SongTitle);
                _cachedSongs[key].Artist = tagFile.GetAttribute(MetaAttribute.Artist);
                _cachedSongs[key].Album = tagFile.GetAttribute(MetaAttribute.Album);
                _cachedSongs[key].Genre = tagFile.GetAttribute(MetaAttribute.Genre);
                _cachedSongs[key].Year = tagFile.GetAttribute(MetaAttribute.Year);
                _cachedSongs[key].TrackNumber = tagFile.GetAttribute(MetaAttribute.TrackNumber);
                _cachedSongs[key].SupportingArtists = tagFile.GetAttribute(MetaAttribute.SupportingArtist);
                SongChanged(_cachedSongs[key]);
            }
        }
        public void DeletedFile(object sender, FileSystemEventArgs e)
        {
            var firstOrDefault = _cachedSongs.Values.FirstOrDefault(x => x.SongPath.Equals(new SongPath(e.FullPath)));
            if (firstOrDefault != null)
            {
                var key = firstOrDefault.Id;
                var oldSong = _cachedSongs[key];
                _cachedFiles.Remove(key);
                _cachedSongs.Remove(key);
                SongDeleted(oldSong);
            }

        }
        public void CreatedFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                var key = Guid.NewGuid().ToString();
                AddFileToCache(key, e.FullPath);
                SongCreated(_cachedSongs[key]);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        
        public event SongRenamedHandler SongRenamed = delegate { };
        public event SongUpdatedHandler SongChanged = delegate { };
        public event SongUpdatedHandler SongCreated = delegate { };
        public event SongUpdatedHandler SongDeleted = delegate { };

        public delegate void SongUpdatedHandler(LocalSong e);
        public delegate void SongRenamedHandler(LocalSong oldSong, LocalSong newSong);
        #endregion

        public AbstractSong GetSong(MediaItem mediaItem)
        {
            return _cachedSongs.Values.FirstOrDefault(x => x.SongPath.Equals(new SongPath(mediaItem.Location)));
        }
    }
    
    public interface IFileManager
    {
        void Initialize(DirectoryPath directory);
        IEnumerable<LocalSong> GetAllSongs();
        void DeleteSong(LocalSong song);
        LocalSong GetSong(SongPath songPath);
        AbstractSong GetSong(MediaItem mediaItem);
        void MoveDirectory(DirectoryPath oldPath, DirectoryPath newPath);
        void MoveFile(SongPath oldPath, SongPath newPath);
        void CopyFile(SongPath oldPath, SongPath newPath);
        bool FileExists(SongPath oldPath);
        bool DirectoryExists(DirectoryPath directory);
        void CreateDirectory(DirectoryPath directory);
        FileInfo GetFileInfo(LocalSong song);

        void UpdateFile(object sender, RenamedEventArgs e);
        void ChangedFile(object sender, FileSystemEventArgs e);
        void DeletedFile(object sender, FileSystemEventArgs e);
        void CreatedFile(object sender, FileSystemEventArgs e);
        void SaveSong(AbstractSong song);
    }
    
}
