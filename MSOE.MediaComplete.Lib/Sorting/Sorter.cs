using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Provides the implementation for sorting the library of music files by metadata
    /// </summary>
    public class Sorter
    {
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
        /// <param name="settings">Sort settings</param>
        public Sorter(SortSettings settings)
        {
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
        public async Sys.Task CalculateActions()
        {
            await Sys.Task.Run(() =>
            {
                var fileInfos = Settings.Files ??
                                Settings.Root.EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles();

                foreach (var file in fileInfos)
                {
                    var path = Settings.Root.PathSegment(file.Directory);
                    var targetFile = GetNewLocation(file, Settings.SortOrder);
                    var targetPath = Settings.Root.PathSegment(targetFile.Directory);

                    // If the current and target paths are different, we know we need to move.
                    if (!path.SequenceEqual(targetPath, new DirectoryEqualityComparer()))
                    {
                        // Check to see if the file already exists
                        var srcMp3File = TagLib.File.Create(file.FullName);
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
        private FileInfo GetNewLocation(FileInfo file, IEnumerable<MetaAttribute> list)
        {
            TagLib.File metadata;
            try
            {
                metadata = TagLib.File.Create(file.FullName);
            }
            catch (TagLib.CorruptFileException)
            {
                // TODO (MC-125) log
                return file; // Bad MP3 - just have it stay in the same place
            }

            var metadataPath = new StringBuilder();
            foreach (var metaValue in list.Select(metadata.StringForMetaAttribute).TakeWhile(metaValue => !String.IsNullOrWhiteSpace((metaValue))))
            {
                metadataPath.Append(metaValue);
                metadataPath.Append(Path.DirectorySeparatorChar);
            }
            return new FileInfo(Settings.Root.FullName + Path.DirectorySeparatorChar + metadataPath.ToString().GetValidFileName() + file.Name);
        }

        #region Import Event Handling

        static Sorter()
        {
            Importer.ImportFinished += SortNewImports;   
        }

        /// <summary>
        /// Sorts incoming files that have just been imported.
        /// </summary>
        /// <param name="results">The results of the triggering import</param>
        public static void SortNewImports (ImportResults results)
        {
            if (!SettingWrapper.GetIsSorting()) return;
            // TODO (MC-43) get settings from configuration
            var settings = new SortSettings
            {
                SortOrder = new List<MetaAttribute> { MetaAttribute.Artist, MetaAttribute.Album },
                Root = results.HomeDir,
                Files = results.NewFiles
            };
            var sorter = new Sorter(settings);
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
            public FileInfo Source { get; set; }
            public FileInfo Dest { get; set; }

            public void Do()
            {
                if (Dest.Directory == null) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                Directory.CreateDirectory(Dest.Directory.FullName);
                File.Move(Source.FullName, Dest.FullName);
            }
        }

        public class DeleteAction : IAction
        {
            public FileInfo Target { get; set; }

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