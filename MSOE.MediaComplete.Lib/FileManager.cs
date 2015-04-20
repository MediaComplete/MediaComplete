using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Songs;
using TaglibFile = TagLib.File;

namespace MSOE.MediaComplete.Lib
{
    public class FileManager : IFileManager
    {
        private readonly Dictionary<string, LocalSong> _cachedLocalSongs;
        private readonly Dictionary<LocalSong, FileInfo> _cachedFileInfos; 
        public static readonly FileManager Instance = new FileManager();

        private FileManager()
        {
            _cachedFileInfos = new Dictionary<LocalSong, FileInfo>();
            _cachedLocalSongs = new Dictionary<string, LocalSong>();
        }

        public void AddToCache(string directory)
        {
            var files = new DirectoryInfo(directory).GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                try
                {
                    AddFileToCache(fileInfo);
                }
                catch (Exception)
                {
                    var song = new LocalSong(fileInfo);
                    _cachedLocalSongs.Add(fileInfo.FullName, song);
                    _cachedFileInfos.Add(song, fileInfo);   
                }
            }
        }

        private void AddFileToCache(FileInfo fileInfo)
        {
            var newFile = GetNewLocalSong(fileInfo);
            _cachedLocalSongs.Add(fileInfo.FullName, newFile);
            _cachedFileInfos.Add(newFile, fileInfo);   
        }

        private LocalSong GetNewLocalSong(FileInfo fileInfo)
        {
            var tagFile = CreateTaglibFile(fileInfo.FullName);
            return new LocalSong(fileInfo)
            {
                Title = tagFile.GetAttribute(MetaAttribute.SongTitle),
                Artist = tagFile.GetAttribute(MetaAttribute.Artist),
                Album = tagFile.GetAttribute(MetaAttribute.Album),
                Genre = tagFile.GetAttribute(MetaAttribute.Genre),
                Year = Convert.ToInt32(tagFile.GetAttribute(MetaAttribute.Year)),
                TrackNumber = Convert.ToInt32(tagFile.GetAttribute(MetaAttribute.TrackNumber)),
                SupportingArtists = tagFile.GetAttribute(MetaAttribute.SupportingArtist),
                Path = fileInfo.FullName
            };
        }
        public FileInfo GetFileInfo(string path)
        {
            return GetFileInfo(_cachedLocalSongs[path]);
        }

        public FileInfo GetFileInfo(LocalSong song)
        {
            return song == null ? null : _cachedFileInfos[song];
        }

        public LocalSong GetSong(string path)
        {
            return _cachedLocalSongs[path];
        }

        public void MoveDirectory(string source, string dest)
        {
            if(Directory.Exists(dest)) throw new IOException("Destination directory already exists");
            var sourceDir = new DirectoryInfo(source);

            var folders = sourceDir.GetDirectories().ToList();
            var files = sourceDir.GetFiles().ToList();

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            folders.ForEach(x => x.MoveTo(dest +Path.DirectorySeparatorChar +x.Name));
            files.ForEach(x => x.MoveTo(dest +Path.DirectorySeparatorChar + x.Name));

            if(sourceDir.GetDirectories().Length == 0 && sourceDir.GetFiles().Length == 0) sourceDir.Delete();

        }
         
        public void CopyFile(SongPath file, SongPath newFile)
        {
            File.Copy(file.FullPath, newFile.FullPath);
        }

        public void MoveFile(FileInfo file, string newFile)
        {
            MoveFile(file.FullName, newFile);
        }

        public void CreateDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
        }
        
        public bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        public void MoveFile(string oldFile, string newFile)
        {
            if (!_cachedLocalSongs.ContainsKey(oldFile)) throw new ArgumentException();
            File.Move(oldFile, newFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public TaglibFile CreateTaglibFile(string fileName) 
        {
             return TagLib.File.Create(fileName);
        }

        public void MoveFile(SongPath oldFile, SongPath newFile)
        {
            MoveFile(oldFile.FullPath, newFile.FullPath);
        }

        public bool FileExists(SongPath fileName)
        {
            return File.Exists(fileName.FullPath);
        }
        
        #region FileWatcher and Events 
        public void UpdateFile(object sender, RenamedEventArgs e)
        {
            var song = _cachedLocalSongs[e.OldFullPath];
            _cachedFileInfos.Remove(song);
            _cachedLocalSongs.Remove(e.OldFullPath);

            song.Path = e.FullPath;
            _cachedLocalSongs.Add(e.FullPath, song);
            _cachedFileInfos.Add(song, new FileInfo(e.FullPath));
            SongRenamed(song, _cachedLocalSongs[e.FullPath]);
        }

        public void ChangedFile(object sender, FileSystemEventArgs e)
        {
            if (_cachedLocalSongs.ContainsKey(e.FullPath))
            {
                var tagFile = CreateTaglibFile(e.FullPath);
                _cachedLocalSongs[e.FullPath].Title = tagFile.GetAttribute(MetaAttribute.SongTitle);
                _cachedLocalSongs[e.FullPath].Artist = tagFile.GetAttribute(MetaAttribute.Artist);
                _cachedLocalSongs[e.FullPath].Album = tagFile.GetAttribute(MetaAttribute.Album);
                _cachedLocalSongs[e.FullPath].Genre = tagFile.GetAttribute(MetaAttribute.Genre);
                _cachedLocalSongs[e.FullPath].Year = Convert.ToInt32(tagFile.GetAttribute(MetaAttribute.Year));
                _cachedLocalSongs[e.FullPath].TrackNumber =
                    Convert.ToInt32(tagFile.GetAttribute(MetaAttribute.TrackNumber));
                _cachedLocalSongs[e.FullPath].SupportingArtists = tagFile.GetAttribute(MetaAttribute.SupportingArtist);
                SongChanged(_cachedLocalSongs[e.FullPath]);
            }
            else
            {
                AddFileToCache(new FileInfo(e.FullPath));
                SongCreated(_cachedLocalSongs[e.FullPath]);
            }
        }
        public void DeletedFile(object sender, FileSystemEventArgs e)
        {
            var oldSong = _cachedLocalSongs[e.FullPath];
            _cachedFileInfos.Remove(oldSong);
            _cachedLocalSongs.Remove(e.FullPath);
            SongDeleted(oldSong);
        }
        public void CreatedFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                AddFileToCache(new FileInfo(e.FullPath));
                SongCreated(_cachedLocalSongs[e.FullPath]);
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

        public static IEnumerable<SongPath> GetSongPaths(string selectedDir)
        {
            var d = new DirectoryInfo(selectedDir);
            return d.EnumerateFiles("*", SearchOption.AllDirectories)
                    .GetMusicFiles().Select(x => new SongPath(x.FullName));
        }
    }

    public interface IFileManager
    {
        void CopyFile(SongPath file, SongPath newFile);
        void MoveFile(FileInfo file, string newFile);
        void MoveFile(string oldFile, string newFile);
        void MoveDirectory(string source, string dest);
        void CreateDirectory(string directory);
        bool FileExists(SongPath fileName);
        bool DirectoryExists(string directory);
        TaglibFile CreateTaglibFile(string fileName);
        void MoveFile(SongPath oldFile, SongPath newFile);
    }
}
