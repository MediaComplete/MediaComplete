using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Files;

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

        private readonly IFileManager _fm;

        /// <summary>
        /// Constructs an Importer with the given library home directory.
        /// </summary>
        /// <param name="fms">FileManager used for dependency injection</param>
        public Importer(IFileManager fms)
        {
            _fm = fms;
        }

        /// <summary>
        /// Helper method to import all the files in a given directory, recursively. If the recursion 
        /// would delve into this Importer's homedir, those files are ignored.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="isCopy">If true, files are copies and the original files remain in the source location. 
        ///     Otherwise, files are "cut" and removed from the source directory.</param>
        /// <returns>An awaitable task of ImportResults</returns>
        public async Task<ImportResults> ImportDirectoryAsync(IEnumerable<SongPath> files, bool isCopy)
        {
            var copyFiles = files.Where(x => !_fm.GetAllSongs().Select(y => y.SongPath).Contains(x));
            var results = await ImportFilesAsync(copyFiles, isCopy);
            return results;
        }

        /// <summary>
        /// Performs an asynchronous import operation.
        /// </summary>
        /// <param name="files">A list of full file paths to be imported</param>
        /// <param name="isMove">If true, files are "cut" and removed from the source directory.
        /// Otherwise, files are copied and the original files remain in the source location.</param>
        /// <returns>An awaitable task of ImportResults</returns>
        /// <exception cref="InvalidImportException">Thrown when files includes a file in the current home directory</exception>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<ImportResults> ImportFilesAsync(IEnumerable<SongPath> files, bool isMove)
        {
            if (files.Any(f => f.HasParent(SettingWrapper.MusicDir)))
            {
                throw new InvalidImportException();
            }

            var task = new ImportTask(_fm, files, isMove);

            Queue.Inst.Add(task);

            task.Done += data =>
            {
                if (task.Results != null)
                {
                    ImportFinished(task.Results);
                }
            };

            await task.Lock.WaitAsync();
            return task.Results;
        }
    }

    /// <summary>
    /// A struct-class containing post-mortem data on an import operation.
    /// </summary>
    public class ImportResults
    {
        public List<SongPath> NewFiles { get; set; } 
        public DirectoryPath HomeDir { get; set; }
        public int FailCount { get; set; }
    }

    /// <summary>
    /// Thrown when an import is attempted on files in the library directory. 
    /// </summary>
    public class InvalidImportException : Exception
    {
        public InvalidImportException() : base("Cannot import a file already located in the library directory tree!") { }
    }
}
