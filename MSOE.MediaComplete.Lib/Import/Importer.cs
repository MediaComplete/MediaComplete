using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaComplete.Lib.Metadata;
using MediaComplete.Lib.Sorting;
using Task = MediaComplete.Lib.Background.Task;
using Sys = System.Threading.Tasks;
using MediaComplete.Lib.Background;
using MediaComplete.Lib.Library.DataSource;

namespace MediaComplete.Lib.Import
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

        /// <summary>
        /// Delegate definition of a post-import callback function
        /// </summary>
        /// <param name="results">The results of the import.</param>
        public delegate void ImportHandler(ImportResults results);

        /// <summary>
        /// Gets or sets the results object.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public ImportResults Results { get; set; }
        private readonly IEnumerable<SongPath> _files;
        private readonly IFileSystem _fileSystem;
        private readonly bool _isMove;

        /// <summary>
        /// Constructs an Importer with the given library home directory.
        /// </summary>
        /// <param name="fileSystem">The file manager to use</param>
        /// <param name="files">The files to import</param>
        /// <param name="isMove">Toggles cut vs. copy behavior</param>
        public Importer(IFileSystem fileSystem, IEnumerable<SongPath> files, bool isMove)
        {
            if (fileSystem == null || files == null) throw new ArgumentNullException();
            _fileSystem = fileSystem;
            _isMove = isMove;
            if (files.Any(f => f.HasParent(SettingWrapper.MusicDir)))
            {
                throw new InvalidImportException();
            }
            _files = files;
            Done += data =>
            {
                if (Results != null)
                {
                    ImportFinished(Results);
                }
            };

        }

        #region Task Overrides
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
                    if (_fileSystem.FileExists(newFile)) continue;
                    try
                    {
                        if (_isMove)
                        {
                            _fileSystem.MoveFile(file, newFile);
                        }
                        else
                        {
                            _fileSystem.CopyFile(file, newFile);
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

        /// <summary>
        /// Contains any subclass types that cannot appear before this task in the execution queue.
        /// Used by <see cref="TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidBeforeTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        /// <summary>
        /// Contains any subclass types that cannot appear after this task in the execution queue.
        /// Used by <see cref="TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidAfterTypes
        {

            get { return new List<Type> { typeof(Sorter), typeof(Identifier) }.AsReadOnly(); }
        }

        /// <summary>
        /// Contains any subclass types that cannot appear in the same parallel block in the execution queue.
        /// Used by <see cref="TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidDuringTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        /// <summary>
        /// Used by subclasses to determine if the task should be removed before adding this one.
        /// </summary>
        /// <param name="t">The other task to consider</param>
        /// <returns>
        /// true if t should be removed, false otherwise
        /// </returns>
        public override bool RemoveOther(Task t)
        {
            return false;
        }
        #endregion
    }

    /// <summary>
    /// Postmortem data on an import operation.
    /// </summary>
    public class ImportResults
    {
        /// <summary>
        /// Gets or sets the new files.
        /// </summary>
        /// <value>
        /// The new files.
        /// </value>
        public List<SongPath> NewFiles { get; set; }

        /// <summary>
        /// Gets or sets the fail count.
        /// </summary>
        /// <value>
        /// The fail count.
        /// </value>
        public int FailCount { get; set; }
    }

    /// <summary>
    /// Thrown when an import is attempted on files in the library directory. 
    /// </summary>
    public class InvalidImportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidImportException"/> class.
        /// </summary>
        public InvalidImportException() : base("Cannot import a file already located in the library directory tree!") { }
    }
}
