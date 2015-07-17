using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Library.FileSystem;

namespace MSOE.MediaComplete.Lib.Sorting
{
    public class Action
    {
        #region Actions

        private static readonly IFileSystem _fileSystem = Dependency.Resolve<IFileSystem>();
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
                if (Dest == null) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                if (!_fileSystem.DirectoryExists(Dest.Directory))
                {
                    _fileSystem.CreateDirectory(Dest.Directory);
                }
                _fileSystem.MoveFile(Source, Dest);
            }
        }

        public class DeleteAction : IAction
        {
            public LocalSong Target { get; set; }

            public void Do()
            {
                if (Target == null || !_fileSystem.FileExists(Target.SongPath)) // Will happen if something goes wrong in the calculation
                {
                    return;
                }

                _fileSystem.DeleteSong(Target); // TODO (MC-74) This should be a "recycle" delete. Not implemented yet.
            }
        }

        #endregion
    }
}
