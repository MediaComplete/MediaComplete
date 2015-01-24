using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static bool IsSorting { get; private set; }

        private readonly DirectoryInfo _root;

        /// <summary>
        /// Creates a sorter with the specified library root and sort settings.
        /// 
        /// This constructor automatically calculates all the necessary changes to file locations, so 
        /// <see cref="Actions">MoveActions</see> can be accessed directly after to anticipate the
        /// magnitude and specifics of the move.
        /// </summary>
        /// <param name="root">The root of the library</param>
        /// <param name="settings">Sort settings</param>
        public Sorter(DirectoryInfo root, SortSettings settings) : this(root)
        {
            if (_root != null)
            {
                CalculateActions(_root.EnumerateFiles("*",SearchOption.AllDirectories).GetMusicFiles(),settings);
            }
        }

        private Sorter(DirectoryInfo root)
        {
            _root = root;
            UnsortableCount = 0;
            Actions = new List<IAction>();
        }

        /// <summary>
        /// Performs the actual movement operations associated with the sort.
        /// </summary>
        /// <returns>A task on completion of the sort. Currently no value is returned.</returns>
        public async Task PerformSort()
        {
            IsSorting = true;
            StatusBarHandler.Instance.ChangeStatusBarMessage("Sorting-Started", StatusBarHandler.StatusIcon.Working);
            await Task.Run(() =>
            {
                foreach (var action in Actions)
                {
                    action.Do();

                    // TODO post timed updates to the status bar
                }
                ScrubEmptyDirectories(_root);
            });
            StatusBarHandler.Instance.ChangeStatusBarMessage("Sorting-Success", StatusBarHandler.StatusIcon.Success);
            IsSorting = false;
        }

        /// <summary>
        /// Private function to determine what movements need to occur to put the library in order
        /// </summary>
        /// <param name="settings">The sorting settings</param>
        /// <param name="files">The files to consider taking action upon</param>
        private void CalculateActions(IEnumerable<FileInfo> files, SortSettings settings)
        {
            var fileInfos = files as IList<FileInfo> ?? files.ToList();
            if (!fileInfos.Any() || settings == null)
            {
                return;
            }

            foreach (var file in fileInfos)
            {
                var path = _root.PathSegment(file.Directory);
                var targetFile = GetNewLocation(file, settings.SortOrder);
                var targetPath = _root.PathSegment(targetFile.Directory);

                // If the current and target paths are different, we know we need to move.
                if (!path.SequenceEqual(targetPath, new DirectoryEqualityComparer()))
                {
                    // Check to see if the file already exists
                    var srcMusicFile = TagLib.File.Create(file.FullName);
                    var destDir = targetFile.Directory;
                    if (destDir.ContainsMusicFile(srcMusicFile)) // If the file is already there
                    {
                        // Delete source, let the older file take precedence.
                        // TODO perhaps we should try comparing audio quality and pick the better one?
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
                if (targetPath.Count != settings.SortOrder.Count + 1)
                {
                    UnsortableCount++;
                }
            }
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
                Console.Out.WriteLine("asdfdfasdfasdfasdf");
                // TODO log
                return file; // Bad music file - just have it stay in the same place
            }

            var metadataPath = new StringBuilder();
            foreach (var metaValue in list.Select(metadata.StringForMetaAttribute).TakeWhile(metaValue => !String.IsNullOrWhiteSpace((metaValue))))
            {
                metadataPath.Append(metaValue);
                metadataPath.Append(Path.DirectorySeparatorChar);
            }
            return new FileInfo(_root.FullName + Path.DirectorySeparatorChar + metadataPath.ToString().GetValidFileName() + file.Name);
        }

        /// <summary>
        /// Removes empty directories and subdirectories from the given root directory
        /// </summary>
        /// <param name="rootInfo">The root of the tree</param>
        private static void ScrubEmptyDirectories(DirectoryInfo rootInfo)
        {
            foreach (var child in rootInfo.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                ScrubEmptyDirectories(child);
                if (!child.EnumerateFileSystemInfos().Any())
                {
                    child.Delete();
                }
            }
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
        public static async void SortNewImports (ImportResults results)
        {
            if (!SettingWrapper.GetIsSorting()) return;
            // TODO get settings from configuration
            var settings = new SortSettings
            {
                SortOrder = new List<MetaAttribute> { MetaAttribute.Artist, MetaAttribute.Album }
            };
            var sorter = new Sorter(results.HomeDir);
            sorter.CalculateActions(results.NewFiles, settings);
            await sorter.PerformSort();
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

                Target.Delete(); // TODO This should be a "recycle" delete. Not implemented yet.
            }
        }

        #endregion
    }
}