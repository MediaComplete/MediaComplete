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
        private Dictionary<string, LocalSong> _cachedLocalSongs;
        private Dictionary<LocalSong, FileInfo> _cachedFileInfos; 
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
                var tagFile = CreateTaglibFile(fileInfo.FullName);
                var newFile = new LocalSong(fileInfo)
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
                _cachedLocalSongs.Add(fileInfo.FullName, newFile);
                _cachedFileInfos.Add(newFile, fileInfo);
            }
        }

        public FileInfo GetFileInfo(string str)
        {
            return GetFileInfo(_cachedLocalSongs[str]);
        }

        public FileInfo GetFileInfo(AbstractSong song)
        {
            return song == null ? null : _cachedFileInfos[(LocalSong) song];
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

        public void CopyFile(FileInfo file, string newFile)
        {
            file.CopyTo(newFile);
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
            var song = _cachedLocalSongs[oldFile];
            _cachedFileInfos.Remove(song);
            _cachedLocalSongs.Remove(oldFile);
            File.Move(oldFile, newFile);
            song.Path = newFile;
            _cachedLocalSongs.Add(newFile, song);
            _cachedFileInfos.Add(song, new FileInfo(newFile));
        }

        public TaglibFile CreateTaglibFile(string fileName)
        {
            return TagLib.File.Create(fileName);
        }
        
        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

    }

    public interface IFileManager
    {
        void CopyFile(FileInfo file, string newFile);
        void MoveFile(FileInfo file, string newFile);
        void MoveFile(string oldFile, string newFile);
        void MoveDirectory(string source, string dest);
        void CreateDirectory(string directory);
        bool FileExists(string fileName);
        bool DirectoryExists(string directory);
        TaglibFile CreateTaglibFile(string fileName);
    }
}
