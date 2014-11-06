using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    class Sorter
    {
        private List<MoveAction> MoveActions { get; set; }

        public Sorter(DirectoryInfo rootDir, SortSettings settings)
        {
            MoveActions = new List<MoveAction>();
            CalculateActions(rootDir, settings);
        }

        public async void PerformSort ()
        {
            await Task.Run(() =>
            {
                foreach (var action in MoveActions)
                {
                    File.Move(action.Source.FullName, action.Dest.FullName);
                }
            });
        }

        private void CalculateActions(DirectoryInfo rootDir, SortSettings settings)
        {

            foreach (FileInfo file in rootDir.EnumerateFiles(Constants.MusicFilePattern, SearchOption.AllDirectories))
            {
                var path = rootDir.PathSegment(file.Directory);

                if (path.Count != settings.SortOrder.Count) {
                    MoveActions.Add(new MoveAction { Source = file, Dest = GetNewLocation(rootDir, file, settings.SortOrder) });
                    continue;
                }

                for (var i = 0; i < path.Count; i++)
                {
                    if (path[i] != file.Parent(i - path.Count))
                    {
                        MoveActions.Add(new MoveAction { Source = file, Dest = GetNewLocation(rootDir, file, settings.SortOrder) });
                        break;
                    }
                }
            }
        }

        private DirectoryInfo GetNewLocation(DirectoryInfo root, FileInfo file, IEnumerable<MetaAttribute> list)
        {
            var metadata = TagLib.File.Create(file.FullName).Tag;
            var metadataPath = String.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture),
                list.Select(metadata.StringForMetaAttribute));
            return new DirectoryInfo(root.FullName + Path.DirectorySeparatorChar + metadataPath);
        }

        private class MoveAction
        {
            public FileInfo Source { get; set; }
            public DirectoryInfo Dest { get; set; }
        }
    }
}
