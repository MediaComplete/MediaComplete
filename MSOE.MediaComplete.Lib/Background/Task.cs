using System.Collections.Generic;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Background
{
    /// <summary>
    /// This abstract class represents a background queue task. 
    /// </summary>
    public abstract class Task
    {
        public delegate void UpdateHandler(Task data);
        /// <summary>
        /// Called by the task when it has a new status, so logs, status bar, etc. can be updated by the queue.
        /// </summary>
        public event UpdateHandler Update = delegate{};

        /// <summary>
        /// The index of this task, as assigned by the work queue.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The amount of the task that has been finished. 
        /// </summary>
        public double PercentComplete { get; set; }
        /// <summary>
        /// The string message reflecting the current status of the task
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The icon reflecting the current status of the task
        /// </summary>
        public StatusBarHandler.StatusIcon Icon { get; set; }

        /// <summary>
        /// Handles the resolution of redundant or blocking jobs. The argument is the current state of the queue,
        /// which is then edited in-place to include this task at the appropriate location, as well as any other 
        /// shifts that need to occur. For more details on how the queue works, see <see cref="Queue"/>
        /// </summary>
        /// <param name="currentQueue">The  work queue</param>
        public abstract void ResolveConflicts(Dictionary<int, List<Task>> currentQueue);
        /// <summary>
        /// Performs the action of this task, asynchronously. 
        /// </summary>
        /// <param name="i">The index of this task, as assigned by the queue when it's started</param>
        /// <returns>An async Task</returns>
        public abstract Sys.Task Do(int i);
    }
}
