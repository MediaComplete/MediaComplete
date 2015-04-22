using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Songs;
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
                var fileInfos = _fileManager.GetAllSongs();

                foreach (var file in fileInfos)
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
                    var path = Settings.Root.PathSegment(file.Path);
                    var targetFile = GetNewLocation(file, Settings.SortOrder);
                    var targetPath = Settings.Root.PathSegment(targetFile.Directory);

                    // If the current and target paths are different, we know we need to move.
                    if (!path.SequenceEqual(targetPath, new DirectoryEqualityComparer()))
                    {
                        // Check to see if the file already exists
                        var srcMp3File = _fileManager.CreateTaglibFile(file.FullName);
                        var destDir = targetFile.Directory;
                        if (destDir.ContainsMusicFile(srcMp3File)) // If the file is already there
                        {
                            // Delete source, let the older file take precedence.
                            // TODO (MC-124) perhaps we should try comparing audio quality and pick the better one?
                            Actions.Add(new DeleteAction
                            {
                                Target = file
                            });
                        }
                        else
                        {
                            Actions.Add(new MoveAction
                            {
                                Source = file,
                                Dest = targetFile
                            });
                        }
                    }

                    // If the target path doesn't fulfill the sort settings, bump the counter.
                    if (targetPath.Count != Settings.SortOrder.Count + 1)
                    {
                        UnsortableCount++;
                    }
                }
            });
        }

        /// <summary>
        /// Calculate the correct location for a file given an ordering of MetaAttributes to sort by.
        /// </summary>
        /// <param name="file">The source file to analyze</param>
        /// <param name="list">The sort-order of meta attributes</param>
        /// <returns>A new FileInfo describing where the source file should be moved to</returns>
        private FileInfo GetNewLocation(LocalSong song, IEnumerable<MetaAttribute> list)
        {
            var metadataPath = new StringBuilder();
            foreach (var metaValue in list.Select(song.GetAttribute))
            {
                metadataPath.Append(metaValue);
                metadataPath.Append(Path.DirectorySeparatorChar);
            }
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
            //TODO TODO RENAMING FILES MOTHERFUCKER
            return
                new FileInfo(SettingWrapper.MusicDir + metadataPath.ToString().GetValidFileName()); // + file);
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
            public FileInfo Dest { get; set; }

            public void Do()
            {
                if (Dest.Directory == null) // Will happen if something goes wrong in the calculation
                {
                    return;
                }
                //TODO TODO FIX THIS SHIT
                _fileManager.CreateDirectory(Dest.Directory.FullName);
                _fileManager.MoveFile(Source.SongPath, Dest.FullName);
            }
        }

        public class DeleteAction : IAction
        {
            public LocalSong Target { get; set; }

            public void Do()
            {
                if (Target == null || !Target.Exists) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                Target.Delete(); // TODO (MC-127) This should be a "recycle" delete. Not implemented yet.
            }
        }

        #endregion
    }
}