using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Library.FileSystem;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Interface for actions, performed by the sorter
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// the function that is called when actions are performed in sequence
        /// </summary>
        void Do();
    }

    /// <summary>
    /// Action defined to move a file, used by the sorter
    /// </summary>
    public class MoveAction : IAction
    {
        private static readonly IFileSystem _fileSystem = Dependency.Resolve<IFileSystem>();
        /// <summary>
        /// The source file to move
        /// </summary>
        public LocalSong Source { get; set; }
        /// <summary>
        /// The destination to move the file to
        /// </summary>
        public SongPath Dest { get; set; }

        /// <summary>
        /// the function that is called when actions are performed in sequence
        /// </summary>
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

    /// <summary>
    /// Action defined to delete a file, used by the sorter
    /// </summary>
    public class DeleteAction : IAction
    {
        private static readonly IFileSystem _fileSystem = Dependency.Resolve<IFileSystem>();
        /// <summary>
        /// the file to be deleted
        /// </summary>
        public LocalSong Target { get; set; }

        /// <summary>
        /// the function that is called when actions are performed in sequence
        /// </summary>
        public void Do()
        {
            if (Target == null || !_fileSystem.FileExists(Target.SongPath)) // Will happen if something goes wrong in the calculation
            {
                return;
            }

            _fileSystem.DeleteFile(Target); // TODO (MC-74) This should be a "recycle" delete. Not implemented yet.
        }
    }
}
