using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Files;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Provides the implementation for sorting the library of music files by metadata
    /// </summary>
    public class Sorter
    {
        private static IFileManager _fileManager;
        public List<IAction> Actions { get; private set; }
        public int UnsortableCount { get; private set; }
        public int MoveCount { get { return Actions.Count(a => a is MoveAction); } }
        public int DupCount { get { return Actions.Count(a => a is DeleteAction); } }
        public SortSettings Settings { get; private set; }

        /// <summary>
        /// Creates a sorter with the specified sort settings.
        /// 
        /// This constructor automatically calculates all the necessary changes to file locations, so 
        /// <see cref="Actions">MoveActions</see> can be accessed directly after to anticipate the
        /// magnitude and specifics of the move.
        /// </summary>
        /// <param name="fileManager"></param>
        /// <param name="settings">Sort settings</param>
        public Sorter(IFileManager fileManager, SortSettings settings)
        {
            _fileManager = fileManager;
            Settings = settings;
            Actions = new List<IAction>();
            UnsortableCount = 0;
        }

        /// <summary>
        /// Performs the actual movement operations associated with the sort.
        /// </summary>
        /// <returns>The background queue tasks, so the status may be observed.</returns>
        public SortingTask PerformSort()
        {
            var task = new SortingTask(this);
            Queue.Inst.Add(task);
            return task;
        }

        /// <summary>
        /// Private function to determine what movements need to occur to put the library in order
        /// </summary>
        public async Sys.Task CalculateActionsAsync()
        {
            await Sys.Task.Run(() =>
            {
                var songs = _fileManager.GetAllSongs();

                foreach (var song in songs)
                {
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO
                    //TODO TODO FIX THIS FUCKING SHIT
                    var sourcePath = song.SongPath;
                    var targetPath = GetNewLocation(song, Settings.SortOrder);

                    // If the current and target paths are different, we know we need to move.
                    if (!sourcePath.Equals(targetPath))
                    {
                        if (_fileManager.FileExists(sourcePath)) // If the file is already there
                        {
                            // Delete source, let the older file take precedence.
                            // TODO (MC-124) perhaps we should try comparing audio quality and pick the better one?
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
                    if (targetPath.FullPath.Remove(0, SettingWrapper.MusicDir.FullPath.Length).Split(Path.DirectorySeparatorChar).Count() != Settings.SortOrder.Count + 1)
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
        private static SongPath GetNewLocation(LocalSong song, IEnumerable<MetaAttribute> list)
        {
            var metadataPath = new StringBuilder();
            foreach (var metaValue in list.Select(song.GetAttribute))
            {
                metadataPath.Append(metaValue);
                metadataPath.Append(Path.DirectorySeparatorChar);
            }
            var track = song.TrackNumber.Length == 1 ? "0" + song.TrackNumber : song.TrackNumber;
            //TODO MC-260 Rename songs/configure song naming
            return new SongPath(SettingWrapper.MusicDir + metadataPath.ToString().GetValidFileName() + track + " " + song.Title + " - " + song.Artist); 
        }

        #region Import Event Handling

        static Sorter()
        {
            Importer.ImportFinished += SortNewImports;
            SettingWrapper.RaiseSettingEvent += Resort;
        }

        private static void Resort()
        {
            if (!SortHelper.GetSorting()) return;

            var settings = new SortSettings
            {
                SortOrder = SettingWrapper.SortOrder,
                
                Files = _fileManager.GetAllSongs().Select(x => x.SongPath)
            };
            var sorter = new Sorter(FileManager.Instance, settings);
            sorter.PerformSort();
        }

        /// <summary>
        /// Sorts incoming files that have just been imported.
        /// </summary>
        /// <param name="results">The results of the triggering import</param>
        public static void SortNewImports(ImportResults results)
        {
            if (!SettingWrapper.IsSorting) return;

            //TODO THIS IS ALSO PRETTY TOTALLY FUCKED
            var settings = new SortSettings
            {
                SortOrder = SettingWrapper.SortOrder,
                Files = results.NewFiles
            };
            var sorter = new Sorter(FileManager.Instance, settings);
            sorter.PerformSort();
        }

        #endregion

        #region Actions

        public interface IAction
        {
            void Do();
        }

        public class MoveAction : IAction
        {
            public LocalSong Source { get; set; }
            public SongPath Dest { get; set; }

            public void Do()
            {
                if (Dest== null) // Will happen if something goes wrong in the calculation
                {
                    return;
                }
                //TODO TODO FIX THIS SHIT
                if (!_fileManager.DirectoryExists(Dest.Directory)) {
                    _fileManager.CreateDirectory(Dest.Directory);
                }
                _fileManager.MoveFile(Source.SongPath, Dest);
            }
        }

        public class DeleteAction : IAction
        {
            public LocalSong Target { get; set; }

            public void Do()
            {
                if (Target == null || !_fileManager.FileExists(Target.SongPath)) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                _fileManager.DeleteSong(Target); // TODO (MC-127) This should be a "recycle" delete. Not implemented yet.
            }
        }

        #endregion
    }
}