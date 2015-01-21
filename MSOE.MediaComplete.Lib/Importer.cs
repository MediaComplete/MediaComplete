using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        public delegate void ImportHandler(ImportResults results);
        public static event ImportHandler ImportFinished = delegate { };


        private readonly DirectoryInfo _homeDir;

        public Importer(string dir)
        {
            _homeDir = new DirectoryInfo(dir);
        }


        public async Task<ImportResults> ImportDirectory(string directory, bool isCopy)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);
            var musicFilesInDirectory =
                new DirectoryInfo(directory).EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles();
            var files = musicFilesInDirectory.Where(s => !s.HasParent(_homeDir));
            var results = await ImportFiles(files, isCopy);
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
            return results;
        }

        public async Task<ImportResults> ImportFiles(IEnumerable<FileInfo> files, bool isCopy)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);

            var fileInfos = files as FileInfo[] ?? files.ToArray();
            if (fileInfos.Any(f => f.HasParent(_homeDir)))
            {
                throw new InvalidImportException();
            }

            var results = new ImportResults
            {
                FailCount = 0,
                NewFiles = new List<FileInfo>(fileInfos.Length),
                HomeDir = _homeDir
            };

            foreach (var file in fileInfos)
            {
                var myFile = file;
                var newFile = _homeDir.FullName + Path.DirectorySeparatorChar + Path.GetFileName(file.Name);
                if (File.Exists(newFile)) continue;
                try
                {
                    if (isCopy)
                    {
                        await Task.Run(() => File.Copy(myFile.FullName, newFile));
                    }
                    else
                    {
                        await Task.Run(() => File.Move(myFile.FullName, newFile));
                    }
                    results.NewFiles.Add(new FileInfo(newFile));
                }
                catch (IOException)
                {
                    results.FailCount++;
                    StatusBarHandler.Instance.ChangeStatusBarMessage("Importing-Error", StatusBarHandler.StatusIcon.Error);
                }

                catch (UnauthorizedAccessException)
                {
                    results.FailCount++;
                    StatusBarHandler.Instance.ChangeStatusBarMessage("UnauthorizedAccess-Error", StatusBarHandler.StatusIcon.Error);
                }

            }
            ImportFinished(results);
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
            return results;
        }
    }

    /// <summary>
    /// A struct-class containing post-mortem data on an import operation.
    /// </summary>
    public class ImportResults
    {
        public List<FileInfo> NewFiles { get; set; }
        public DirectoryInfo HomeDir { get; set; }
        public int FailCount { get; set; }
    }

    /// <summary>
    /// Thrown when an import is attempted on files in the library directory. 
    /// </summary>
    public class InvalidImportException : Exception
    {
        public InvalidImportException()
            : base("Cannot import a file already located in the library directory tree!")
        {

        }
    }
}
