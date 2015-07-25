using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Logging;
using MSOE.MediaComplete.Lib.Metadata;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Provides the implementation for sorting the library of music files by metadata
    /// </summary>
    public class Sorter : Task
    {
        /// <summary>
        /// Gets the planned actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public List<IAction> Actions { get; private set; }

        /// <summary>
        /// Gets the number of un-sortable files
        /// </summary>
        /// <value>
        /// The un-sortable count.
        /// </value>
        public int UnsortableCount { get; private set; }

        /// <summary>
        /// Gets the number of files that will be moved
        /// </summary>
        /// <value>
        /// The move count.
        /// </value>
        public int MoveCount { get { return Actions.Count(a => a is MoveAction); } }

        /// <summary>
        /// Gets the number of duplicate files found (which will be deleted)
        /// </summary>
        /// <value>
        /// The duplicate count.
        /// </value>
        public int DupCount { get { return Actions.Count(a => a is DeleteAction); } }

        private static IFileManager _fileManager;

        /// <summary>
        /// The specific files to sort
        /// </summary>
        public IEnumerable<SongPath> Files { get; set; }

        /// <summary>
        /// Creates a sorter with the specified sort settings.
        /// 
        /// This constructor automatically calculates all the necessary changes to file locations, so 
        /// <see cref="Actions">MoveActions</see> can be accessed directly after to anticipate the
        /// magnitude and specifics of the move.
        /// </summary>
        /// <param name="fileManager"></param>
        /// <param name="files"></param>
        public Sorter(IFileManager fileManager, IEnumerable<SongPath> files)
        {
            _fileManager = fileManager;
            Files = files;
            Actions = new List<IAction>();
            UnsortableCount = 0;
        }

        /// <summary>
        /// Private function to determine what movements need to occur to put the library in order
        /// </summary>
        public async Sys.Task CalculateActionsAsync()
        {
            await Sys.Task.Run(() =>
            {
                var songs = _fileManager.GetAllSongs().Where(x => Files.Contains(x.SongPath));
                UnsortableCount += Files.Count() - songs.Count();
                foreach (var song in songs)
                {
                    var sourcePath = song.SongPath;
                    var targetPath = GetNewLocation(song, SettingWrapper.SortOrder);
                    // If the current and target paths are different, we know we need to move.
                    if (!sourcePath.Equals(targetPath))
                    {
                        if (_fileManager.FileExists(targetPath)) // If the file is already there
                        {
                            // Delete source, let the older file take precedence.
                            // TODO (MC-29) perhaps we should try comparing audio quality and pick the better one?
                            Actions.Add(new DeleteAction
                            {
                                Target = song
                            });
                        }
                        else
                        {
                            Actions.Add(new MoveAction
                            {
                                Source = song,
                                Dest = targetPath
                            });
                        }
                    }

                    // If the target path doesn't fulfill the sort settings, bump the counter.
                    if (targetPath.FullPath.Remove(0, SettingWrapper.MusicDir.Length).Split(Path.DirectorySeparatorChar).Count() != SettingWrapper.SortOrder.Count + 1)
                    {
                        UnsortableCount++;
                    }
                }
            });
        }

        /// <summary>
        /// Calculate the correct location for a file given an ordering of MetaAttributes to sort by.
        /// </summary>
        /// <param name="song">The source file to analyze</param>
        /// <param name="list">The sort-order of meta attributes</param>
        /// <returns>A new FileInfo describing where the source file should be moved to</returns>
        private static SongPath GetNewLocation(LocalSong song, IReadOnlyList<MetaAttribute> list)
        {
            var path = "";
            // This is using an indexed for loop for a reason.
            // A for-each loop creates an enumerator. every time it's iterated, it advances to the next value. 
            // Therefore on the first run through, enumerator.current returns the second element of the list.
            // This breaks the naming.
            for (var x = 0; x < list.Count(); x ++)
            {
                var metaValue = song.GetAttributeStr(list[x]);
                var useableValue = metaValue ?? "Unknown " + list[x];
                path += useableValue;
                path += Path.DirectorySeparatorChar;
            }
            //TODO MC-53 Rename songs/configure song naming
            return new SongPath(SettingWrapper.MusicDir.FullPath + GetValidFileName(path) + song.Name); 
        }

        /// <summary>
        /// Fix up a file name so it becomes usable
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetValidFileName(string path)
        {
            //special chars not allowed in filename 
            const string specialChars = @"/:*?""<>|#%&.{}~";

            //Replace special chars in raw filename with empty spaces to make it valid  
            Array.ForEach(specialChars.ToCharArray(), specialChar => path = path.Replace(specialChar.ToString(CultureInfo.CurrentCulture), ""));

            return path;
        }
        #region Task Overrides

        /// <summary>
        /// Performs the sort, calculating the necessary actions first, if necessary.
        /// </summary>
        /// <param name="i">The task identifier</param>
        /// <returns>An awaitable task</returns>
        public override void Do(int i)
        {
            try
            {
                Id = i;
                Message = "Sorting-InProgress";
                Icon = StatusBarHandler.StatusIcon.Working;

                if (Actions.Count == 0)
                {
                    Sys.Task.Run(() => CalculateActionsAsync()).Wait();
                }

                var counter = 0;
                var max = (Actions.Count > 100 ? Actions.Count / 100 : 1);
                var total = 0;
                foreach (var action in Actions)
                {
                    try
                    {
                        action.Do();
                    }
                    catch (IOException e)
                    {
                        Error = e;
                        Message = "Sorting-HadError";
                        Icon = StatusBarHandler.StatusIcon.Error;
                        TriggerUpdate(this);
                    }

                    total++;
                    if (counter++ >= max)
                    {
                        counter = 0;
                        PercentComplete = ((double)total) / Actions.Count;
                        TriggerUpdate(this);
                    }
                }

                if (Error == null)
                {
                    Icon = StatusBarHandler.StatusIcon.Success;
                }
            }
            catch (TagLib.CorruptFileException e)
            {
                Logger.LogException("Taglib found a corrupt file while creating the taglib file.", e);
                Message = "Sorting-HadError";
                Icon = StatusBarHandler.StatusIcon.Error;
                Error = e;
            }
            catch (Exception e)
            {
                Message = "Sorting-HadError";
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
            get { return new List<Type> { typeof(Identifier), typeof(Importer) }.AsReadOnly(); }
        }

        /// <summary>
        /// Contains any subclass types that cannot appear after this task in the execution queue.
        /// Used by <see cref="TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidAfterTypes
        {
            get { return new List<Type>().AsReadOnly(); }
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
        /// Removes any other sorters in the queue
        /// </summary>
        /// <param name="t">The other task to consider</param>
        /// <returns>
        /// true if t should be removed, false otherwise
        /// </returns>
        public override bool RemoveOther(Task t)
        {
            return t is Sorter;
        }
        #endregion

        #region Actions

        /// <summary>
        /// Interface describing a potential file operation the sorter is planning
        /// </summary>
        public interface IAction
        {
            /// <summary>
            /// Executes the operation
            /// </summary>
            void Do();
        }

        /// <summary>
        /// Represents a planned file move operation
        /// </summary>
        public class MoveAction : IAction
        {
            /// <summary>
            /// Gets or sets the source file
            /// </summary>
            /// <value>
            /// The source.
            /// </value>
            public LocalSong Source { get; set; }

            /// <summary>
            /// Gets or sets the destination file
            /// </summary>
            /// <value>
            /// The destination
            /// </value>
            public SongPath Dest { get; set; }

            /// <summary>
            /// Moves the file
            /// </summary>
            public void Do()
            {
                if (Dest== null) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                if (!_fileManager.DirectoryExists(Dest.Directory)) {
                    _fileManager.CreateDirectory(Dest.Directory);
                }
                _fileManager.MoveFile(Source, Dest);
            }
        }

        /// <summary>
        /// Represents a planned file deletion
        /// </summary>
        public class DeleteAction : IAction
        {
            /// <summary>
            /// Gets or sets the file to be deleted
            /// </summary>
            /// <value>
            /// The target.
            /// </value>
            public LocalSong Target { get; set; }

            /// <summary>
            /// Deletes the file
            /// </summary>
            public void Do()
            {
                if (Target == null || !_fileManager.FileExists(Target.SongPath)) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                _fileManager.DeleteSong(Target); // TODO (MC-74) This should be a "recycle" delete. Not implemented yet.
            }
        }

        #endregion
    }
}