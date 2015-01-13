using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Background;

namespace MSOE.MediaComplete.Lib.Import
{
    /// <summary>
    /// Provides methods for adding new files into the application's library.
    /// </summary>
    public class Importer
    {
        /// <summary>
        /// This event is fired whenever any import completes. It is intended to be used for any 
        /// sorting, identification, etc. that takes places on new files.
        /// </summary>
        public static event ImportHandler ImportFinished = delegate {};
        public delegate void ImportHandler(ImportResults results);

        private readonly DirectoryInfo _homeDir;

        /// <summary>
        /// Constructs an Importer with the given library home directory.
        /// </summary>
        /// <param name="dir">The full path to the library home directory.</param>
        public Importer(string dir)
        {
            _homeDir = new DirectoryInfo(dir);
        }

        /// <summary>
        /// Helper method to import all the files in a given directory, recursively. If the recursion 
        /// would delve into this Importer's homedir, those files are ignored.
        /// </summary>
        /// <param name="directory">The full path to the target directory</param>
        /// <param name="isCopy">If true, files are copies and the original files remain in the source location. 
        /// Otherwise, files are "cut" and removed from the source directory.</param>
        /// <returns>An awaitable task of ImportResults</returns>
        public async Task<ImportResults> ImportDirectory(string directory, bool isCopy)
        {
            var files = Array.FindAll(Directory.GetFiles(directory, "*.mp3", SearchOption.AllDirectories),
                s => !new FileInfo(s).HasParent(_homeDir));
            var results = await ImportFiles(files, isCopy);
            return results;
        }

        /// <summary>
        /// Performs an asynchronous import operation.
        /// </summary>
        /// <param name="files">A list of full file paths to be imported</param>
        /// <param name="isCopy">If true, files are copies and the original files remain in the source location. 
        /// Otherwise, files are "cut" and removed from the source directory.</param>
        /// <returns>An awaitable task of ImportResults</returns>
        /// <exception cref="InvalidImportException">Thrown when files includes a file in the current home directory</exception>
        public async Task<ImportResults> ImportFiles(string[] files, bool isCopy)
        {
            // TODO pipe through queue (MC-115)
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);

            var task = new ImportTask(_homeDir, files, isCopy);
            Queue.Inst.Add(task);

            await task.Lock.WaitAsync();

            if (task.Error != null)
            {
                throw task.Error;
            } 
            
            if (task.Results != null)
            {
                ImportFinished(task.Results);
            }

            // TODO pipe through queue (MC-115)
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
            return task.Results;
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
        public InvalidImportException() : base("Cannot import a file already located in the library directory tree!")
        {
        
        }
    }
}
