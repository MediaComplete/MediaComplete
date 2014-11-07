using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Provides the implementation for sorting the library of MP3 files by metadata
    /// </summary>
    public class Sorter
    {
        public List<MoveAction> MoveActions { get; private set; }
        public int UnsortableCount { get; private set; }

        private readonly DirectoryInfo _root;

        /// <summary>
        /// Creates a sorter with the specified library root and sort settings.
        /// 
        /// This constructor automatically calculates all the necessary changes to file locations, so 
        /// <see cref="MoveActions">MoveActions</see> can be accessed directly after to anticipate the
        /// magnitude and specifics of the move.
        /// </summary>
        /// <param name="root">The root of the library</param>
        /// <param name="settings">Sort settings</param>
        public Sorter(DirectoryInfo root, SortSettings settings)
        {
            _root = root;
            MoveActions = new List<MoveAction>();
            UnsortableCount = 0;
            CalculateActions(root, settings); // TODO investigation - could this lock up the GUI?
        }

        /// <summary>
        /// Performs the actual movement operations associated with the sort.
        /// </summary>
        /// <returns>A task on completion of the sort. Currently no value is returned.</returns>
        public async Task PerformSort()
        {
            await Task.Run(() =>
            {
                foreach (var action in MoveActions.Where(action => action.Dest.Directory != null))
                {
                    if (action.Dest.Exists) // If there's duplicate files in the collection. 
                    {
                        // Delete source, let the older one take precedence.
                        // TODO perhaps we should try comparing quality?
                        // TODO This should be a "recycle" delete. Not implemented yet.
                        action.Source.Delete();
                        continue;
                    }
                    if (action.Dest.Directory != null) Directory.CreateDirectory(action.Dest.Directory.FullName);
                    File.Move(action.Source.FullName, action.Dest.FullName);
                }
                ScrubEmptyDirectories(_root);
            });
        }

        /// <summary>
        /// Private function to determine what movements need to occur to put the library in order
        /// </summary>
        /// <param name="rootDir">The root of the library</param>
        /// <param name="settings">The sorting settings</param>
        private void CalculateActions(DirectoryInfo rootDir, SortSettings settings)
        {
            foreach (var file in rootDir.EnumerateFiles(Constants.MusicFilePattern, SearchOption.AllDirectories))
            {
                var path = rootDir.PathSegment(file.Directory);
                var idealPath = GetNewLocation(rootDir, file, settings.SortOrder);

                // If there's just a different number of directories/MetaAttributes, we know it's wrong
                if (path.Count != settings.SortOrder.Count + 1)
                {
                    if (!idealPath.Directory.DirectoryEquals(file.Directory))
                    {
                        MoveActions.Add(new MoveAction
                        {
                            Source = file,
                            Dest = idealPath
                        });
                        continue;
                    }
                        UnsortableCount++;
                }
                
                // Otherwise, calculate the appropriate path from the tags, and compare to the actual path.
                if (path.Where((t, i) => !t.DirectoryEquals(idealPath.Parent(path.Count - i - 1))).Any())
                {
                    MoveActions.Add(new MoveAction
                    {
                        Source = file,
                        Dest = idealPath
                    });
                }
            }
        }

        /// <summary>
        /// Calculate the correct location for a file given an ordering of MetaAttributes to sort by.
        /// </summary>
        /// <param name="root">Root of the metadata folder pathing</param>
        /// <param name="file">The source file to analyze</param>
        /// <param name="list">The sort-order of meta attributes</param>
        /// <returns>A new FileInfo describing where the source file should be moved to</returns>
        private static FileInfo GetNewLocation(FileSystemInfo root, FileSystemInfo file, IEnumerable<MetaAttribute> list)
        {
            var metadata = TagLib.File.Create(file.FullName);
            var metadataPath = new StringBuilder();
            foreach (var metaValue in list.Select(metadata.StringForMetaAttribute).TakeWhile(metaValue => !String.IsNullOrWhiteSpace((metaValue))))
            {
                metadataPath.Append(metaValue);
                metadataPath.Append(Path.DirectorySeparatorChar);
            }
            return new FileInfo(root.FullName + Path.DirectorySeparatorChar + metadataPath + file.Name);
        }

        /// <summary>
        /// Removes empty directories and subdirectories from the given root directory
        /// </summary>
        /// <param name="rootInfo">The root of the tree</param>
        private static void ScrubEmptyDirectories(DirectoryInfo rootInfo)
        {
            foreach (var child in rootInfo.EnumerateDirectories())
            {
                ScrubEmptyDirectories(child);
                if (!child.EnumerateFileSystemInfos().Any())
                {
                    child.Delete();
                }
            }
        }

        /// <summary>
        /// Small, simple class that contains a source and destination file. 
        /// </summary>
        public class MoveAction
        {
            public FileInfo Source { get; set; }
            public FileInfo Dest { get; set; }
        }
    }
}