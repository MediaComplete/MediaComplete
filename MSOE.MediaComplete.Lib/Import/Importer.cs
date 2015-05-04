using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;
using Task = MSOE.MediaComplete.Lib.Background.Task;

namespace MSOE.MediaComplete.Lib.Import
{
    /// <summary>
    /// Provides methods for adding new files into the application's library.
    /// </summary>
    public class Importer : Task
    {
        /// <summary>
        /// This event is fired whenever any import completes. It is intended to be used for any 
        /// sorting, identification, etc. that takes places on new files.
        /// </summary>
        public static event ImportHandler ImportFinished = delegate {};
        public delegate void ImportHandler(ImportResults results);

        public ImportResults Results { get; set; }
        private IEnumerable<SongPath> _files; 
        private readonly IFileManager _fm;
        private bool _isMove;

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
            _isMove = isMove;
            if (files.Any(f => f.HasParent(SettingWrapper.MusicDir)))
            {
                throw new InvalidImportException();
            }

            _files = files;

            Queue.Inst.Add(this);

            Done += data =>
            {
                if (Results != null)
                {
                    ImportFinished(Results);
                }
            };

            await Lock.WaitAsync();
            return Results;
        }
        /// <summary>
        /// Performs the import operation in the background.
        /// </summary>
        /// <param name="i">The task ID, assigned by the queue</param>
        /// <returns>A system task that can be tracked</returns>
        public override void Do(int i)
        {
            Id = i;
            Message = "Import-InProgress";
            Icon = StatusBarHandler.StatusIcon.Working;
            var count = _files.Count();
            var results = new ImportResults
            {
                FailCount = 0,
                NewFiles = new List<SongPath>(count),
            };

            try
            {
                var counter = 0;
                var max = (count > 100 ? count / 100 : 1);
                var total = 0;
                foreach (var file in _files)
                {
                    var newFile = new SongPath(SettingWrapper.MusicDir.FullPath + file.Name);
                    if (_fm.FileExists(newFile)) continue;
                    try
                    {
                        if (_isMove)
                        {
                            _fm.AddFile(file, newFile);
                        }
                        else
                        {
                            _fm.CopyFile(file, newFile);
                        }
                        results.NewFiles.Add(newFile);
                    }
                    catch (IOException exception)
                    {
                        Console.WriteLine(exception); // TODO (MC-125) log
                        results.FailCount++;
                        Message = "Importing-Error";
                        Icon = StatusBarHandler.StatusIcon.Error;
                        Error = exception;
                        TriggerUpdate(this);
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        Console.WriteLine(exception); // TODO (MC-125) log
                        results.FailCount++;
                        Message = "UnauthorizedAccess-Error";
                        Icon = StatusBarHandler.StatusIcon.Error;
                        Error = exception;
                        TriggerUpdate(this);
                    }

                    total++;
                    if (counter++ >= max)
                    {
                        counter = 0;
                        PercentComplete = ((double)total) / count;
                        TriggerUpdate(this);
                    }
                }

                Results = results;
                if (Error == null)
                {
                    Message = "Import-Success";
                    Icon = StatusBarHandler.StatusIcon.Success;
                }
            }
            catch (Exception e)
            {
                Message = "Importing-Error";
                Icon = StatusBarHandler.StatusIcon.Error;
                Error = e;
            }
            finally
            {
                TriggerDone(this);
            }
        }

        #region Task Overrides
        public override IReadOnlyCollection<Type> InvalidBeforeTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        public override IReadOnlyCollection<Type> InvalidAfterTypes
        {

            get { return new List<Type> { typeof(Sorter), typeof(IdentifierTask) }.AsReadOnly(); }
        }

        public override IReadOnlyCollection<Type> InvalidDuringTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        public override bool RemoveOther(Task t)
        {
            return false;
        }
        #endregion
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
