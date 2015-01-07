using System.Collections.Generic;
using System.Linq;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Background
{
    /// <summary>
    /// Class to manage long-running background operations. Will run tasks in parallel where possible, 
    /// and block otherwise, based on the implemenatation of the tasks passed in. This class is a singleton.
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// The singleton instance available to callers.
        /// </summary>
        public static Queue Inst { get; private set; }

        /// <summary>
        /// Private constructor, creates an empty queue.
        /// </summary>
        private Queue()
        {
            _tasks = new List<List<Task>>();
        }
        static Queue()
        {
            Inst = new Queue();
        }

        // The queue of jobs, as integer-index enumurables. This allows groups of tasks to be run in parallel
        private readonly List<List<Task>> _tasks;
        // The number of tasks currently active (at the last spawn).
        private int _activeCount;

        /// <summary>
        /// Adds a new task to the queue. Queued up tasks are shuffled/updated as necessary.
        /// </summary>
        /// <param name="newTask">The new task object</param>
        public void Add(Task newTask)
        {
            lock (_tasks)
            {
                newTask.ResolveConflicts(_tasks);
            }

            if (_activeCount < 1)
            {
                Sys.Task.Run(() => Run());
            }
        }

        /// <summary>
        /// Runs tasks until the queue is empty. Execution starts from the lowest key in the queue, 
        /// and runs the entire enumerable there in parallel.
        /// </summary>
        private void Run()
        {
            while (_tasks.Count > 0)
            {
                List<Task> tasks;
                lock (_tasks)
                {
                    tasks = _tasks.First();
                    _tasks.Remove(tasks);
                }
                _activeCount = tasks.Count;
                tasks.ForEach(t => t.Update += RouteMessage);
                
                Sys.Parallel.For(0, tasks.Count, i => tasks[i].Do(i));
            }
            _activeCount = 0;
        }

        /// <summary>
        /// Sends an update from a background task on to the status bar.
        /// </summary>
        /// <param name="task">The task sending the update, which will have all the necessary data</param>
        private void RouteMessage(Task task)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("[{1}/{2}] {0} ({3}%)",
                task.Message, task.Icon, task.Index, _tasks.Count + _activeCount, task.PercentComplete);
        }
    }
}
