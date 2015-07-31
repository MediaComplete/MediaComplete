using MSOE.MediaComplete.Lib.Library.DataSource;

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
        private static readonly IFileSystem FileSystem = Dependency.Resolve<IFileSystem>();
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

            if (!FileSystem.DirectoryExists(Dest.Directory))
            {
                FileSystem.CreateDirectory(Dest.Directory);
            }
            FileSystem.MoveFile(Source, Dest);
        }
    }

    /// <summary>
    /// Action defined to delete a file, used by the sorter
    /// </summary>
    public class DeleteAction : IAction
    {
        private static readonly IFileSystem FileSystem = Dependency.Resolve<IFileSystem>();
        /// <summary>
        /// the file to be deleted
        /// </summary>
        public LocalSong Target { get; set; }

        /// <summary>
        /// the function that is called when actions are performed in sequence
        /// </summary>
        public void Do()
        {
            if (Target == null || !FileSystem.FileExists(Target.SongPath)) // Will happen if something goes wrong in the calculation
            {
                return;
            }

            FileSystem.DeleteSong(Target); // TODO (MC-74) This should be a "recycle" delete. Not implemented yet.
        }
    }
}
