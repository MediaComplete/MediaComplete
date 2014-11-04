using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace MSOE.MediaComplete.Lib
{
    class Sorter
    {
        private List<MoveAction> MoveActions { get; set; }

        public Sorter(DirectoryInfo rootDir, SortSettings settings)
        {
            MoveActions = new List<MoveAction>();
            calculateActions(rootDir, settings);
        }

        private void calculateActions(DirectoryInfo rootDir, SortSettings settings)
        {

            foreach (FileInfo file in rootDir.EnumerateFiles(Constants.MusicFilePattern, SearchOption.AllDirectories))
            {
                var path = rootDir.PathSegment(file.Directory);

                if (path.Count != settings.SortOrder.Count) {
                    MoveActions.Add(new MoveAction(){Source = file, Dest = getNewLocation(file, settings.SortOrder)});
                    continue;
                }

                for (var i = 0; i < path.Count; i++)
                {
                    if (path[i] != file.Parent(i - path.Count))
                    {
                        MoveActions.Add(new MoveAction() { Source = file, Dest = getNewLocation(file, settings.SortOrder) });
                        continue;
                    }
                }
            }
        }

        private FileInfo getNewLocation(FileInfo file, List<MetaAttribute> list)
        {
            throw new NotImplementedException();
        }

        private class MoveAction
        {
            public FileInfo Source { get; set; }
            public FileInfo Dest { get; set; }
        }
    }
}
