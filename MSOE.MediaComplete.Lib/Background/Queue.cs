using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using MSOE.MediaComplete.Lib.Background;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Background
{
    /// <summary>
    /// Class to manage long-running background operations. Will run tasks in parallel where possible, 
    /// and block otherwise, based on the implemenatation of the tasks passed in. This class is a singleton.
    /// </summary>
    public class Queue : IQueue
    {
        /// <summary>
        /// Private constructor, creates an empty queue.
        /// </summary>
        public Queue()
        {
            _tasks = new List<List<Task>>();
        }

        #region Privates
        // The queue of jobs, as integer-index enumurables. This allows groups of tasks to be run in parallel
        private readonly List<List<Task>> _tasks;
        // The number of tasks currently active (at the last spawn).
        private int _activeCount;
        // The highest-importance task received in a session (i.e. the one with the highest-priority icon)
        private Task _greatestTask;
        // The total number of tasks in this session
        private int _sessionCount;
        #endregion

        /// <summary>
        /// Adds a new task to the queue. Queued up tasks are shuffled/updated as necessary.
        /// </summary>
        /// <param name="newTask">The new task object</param>
        public void Add(Task newTask)
        {
            lock (_tasks)
            {
                TaskAdder.ResolveConflicts(newTask, _tasks);
            }

            if (_activeCount < 1)
            {
                Sys.Task.Run(() => Run());
            }
            _sessionCount++;
        }

        /// <summary>
        /// Runs tasks until the queue is empty. Execution starts from the lowest key in the queue, 
        /// and runs the entire enumerable there in parallel.
        /// </summary>
        private void Run()
        {
            var id = 1;
            var keepGoing = true;
            while (keepGoing)
            {
                List<Task> tasks;
                lock (_tasks)
                {
                    tasks = _tasks.First();
                    _tasks.Remove(tasks);
                }
                _activeCount = tasks.Count;
                tasks.ForEach(t => t.Update += RouteMessage);
                
                Sys.Parallel.For(0, tasks.Count, j => tasks[j].Do(id++));

                lock (_tasks)
                {
                    keepGoing = _tasks.Count > 0;
                }
            }

            ResetSession();
        }

        /// <summary>
        /// Resets any variables used over the course of this session.
        /// </summary>
        private void ResetSession()
        {
            _activeCount = 0;
            _sessionCount = 0;
            _greatestTask = null;
        }

        /// <summary>
        /// Sends an update from a background task on to the status bar.
        /// </summary>
        /// <param name="task">The task sending the update, which will have all the necessary data</param>
        private void RouteMessage(Task task)
        {
            if (task.Icon > StatusBarHandler.StatusIcon.Success && (_greatestTask == null || _greatestTask.Icon < task.Icon))
            {
                _greatestTask = task;
            }

            var t = _greatestTask ?? task;

            StatusBarHandler.Instance.ChangeStatusBarMessage("[{1}/{2}] {0} ({3}%)",
                t.Message, t.Icon, t.Id, _sessionCount, (t.PercentComplete * 100).ToString("N1"));
        }
    }
}

public interface IQueue
{
    void Add(Task newTask);
}
