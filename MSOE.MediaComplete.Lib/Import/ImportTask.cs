using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Import
{
    /// <summary>
    /// The background wrapper for a music file import operation
    /// </summary>
    public class ImportTask : Task
    {
        private readonly DirectoryInfo _homeDir;
        private readonly IEnumerable<FileInfo> _files;
        private readonly bool _isCopy;
        private readonly IFileMover _fileMover;

        public ImportResults Results { get; set; }

        /// <summary>
        /// Constructs an Import background task with the given parameters
        /// </summary>
        /// <param name="fileMover"></param>
        /// <param name="homeDir">The target library directory</param>
        /// <param name="files">An array of absolute filepaths to bring in.</param>
        /// <param name="isCopy">If true, files will be cut, else files will be copied</param>
        public ImportTask(IFileMover fileMover, DirectoryInfo homeDir, IEnumerable<FileInfo> files, bool isCopy)
        {
            _fileMover = fileMover;
            Results = null;
            _homeDir = homeDir;
            _files = files;
            _isCopy = isCopy;
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
                NewFiles = new List<FileInfo>(count),
                HomeDir = _homeDir
            };

            try
            {
                var counter = 0;
                var max = (count > 100 ? count / 100 : 1);
                var total = 0;
                foreach (var file in _files)
                {
                    var newFile = _homeDir.FullName + Path.DirectorySeparatorChar + file.Name;
                    if (_fileMover.FileExists(newFile)) continue;
                    try
                    {
                        if (_isCopy)
                        {
                            _fileMover.CopyFile(file, newFile);
                        }
                        else
                        {
                            _fileMover.MoveFile(file, newFile);
                        }
                        results.NewFiles.Add(new FileInfo(newFile));
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
            
            get { return new List<Type> { typeof(SortingTask), typeof(IdentifierTask) }.AsReadOnly(); }
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
}
