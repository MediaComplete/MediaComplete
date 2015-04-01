using System.IO;
using System.Linq;
using TaglibFile = TagLib.File;

namespace MSOE.MediaComplete.Lib
{
    public class FileMover : IFileMover
    {
        public static readonly FileMover Instance = new FileMover();
        private FileMover() { }
        
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
            file.MoveTo(newFile);
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
            File.Move(oldFile, newFile);
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

    public interface IFileMover
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
