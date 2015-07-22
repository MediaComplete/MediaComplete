using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Library.FileSystem;
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
        private static IFileSystem _fileSystem;
        public List<Action.IAction> Actions { get; private set; }
        public int UnsortableCount { get; private set; }
        public int MoveCount { get { return Actions.Count(a => a is Action.MoveAction); } }
        public int DupCount { get { return Actions.Count(a => a is Action.DeleteAction); } }
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
        /// <param name="library"></param>
        /// <param name="files"></param>
        public Sorter(IFileSystem fileSystem, IEnumerable<SongPath> files)
        {
            _fileSystem = fileSystem;
            Files = files;
            Actions = new List<Action.IAction>();
            UnsortableCount = 0;
        }

        /// <summary>
        /// Private function to determine what movements need to occur to put the library in order
        /// </summary>
        public async Sys.Task CalculateActionsAsync()
        {
            await Sys.Task.Run(() =>
            {
                var songs = _fileSystem.GetAllSongFiles().Where(x => Files.Contains(x.SongPath));
                UnsortableCount += Files.Count() - songs.Count();
                foreach (var song in songs)
                {
                    var sourcePath = song.SongPath;
                    var targetPath = GetNewLocation(song, SettingWrapper.SortOrder);
                    // If the current and target paths are different, we know we need to move.
                    if (!sourcePath.Equals(targetPath))
                    {
                        if (_fileSystem.FileExists(targetPath)) // If the file is already there
                        {
                            // Delete source, let the older file take precedence.
                            // TODO (MC-29) perhaps we should try comparing audio quality and pick the better one?
                            Actions.Add(new Action.DeleteAction
                            {
                                Target = song
                            });
                        }
                        else
                        {
                            Actions.Add(new Action.MoveAction
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
            // A foreach loop creates an enumerator. every time it's iterated, it advances to the next value. 
            // Therefore on the first runthrough, enumerator.current returns the second element of the list.
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

        public override IReadOnlyCollection<Type> InvalidBeforeTypes
        {
            get { return new List<Type> { typeof(Identifier), typeof(Importer) }.AsReadOnly(); }
        }

        public override IReadOnlyCollection<Type> InvalidAfterTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        public override IReadOnlyCollection<Type> InvalidDuringTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        public override bool RemoveOther(Task t)
        {
            return t is Sorter;
        }
        #endregion

    }
}