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
    public class FileSystem : IFileSystem
    {
        public static FileSystem instance = new FileSystem();
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

        // TODO MC-35 keep directories and files that aren't music, so they can be managed in-app
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
        /// Moves a file from an old location to a new one
        /// Move operation is performed on the stored FileInfo, and the 
        /// stored song's Path object is updated as well.
        /// </summary>
        /// <param name="oldFile">song that needs to be moved</param>
        /// <param name="newFile">expected location of the file.</param>
        /// <throws>ArgumentException if file does not exist in cache</throws>
        public void MoveFile(FileInfo oldFile, SongPath newFile)
        {
            oldFile.MoveTo(newFile.FullPath);
            
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
    }

    public interface IFileSystem
    {
    }
}
