using System.Collections.Generic;
using System.Linq;
using MSOE.MediaComplete.Lib.Logging;

namespace MSOE.MediaComplete.Lib.Background
{
    /// <summary>
    /// Handles the resolution of potential conflicts between various tasks.
    /// </summary>
    public static class TaskAdder
    {
        /// <summary>
        /// Handles the resolution of redundant or blocking jobs, and adds the passed in Task to the underlying _tasks.
        /// </summary>
        /// <param name="newTask">The new task to add</param>
        /// <param name="currentQueue">The queue to add the new task to</param>
        public static void ResolveConflicts(Task newTask, List<List<Task>> currentQueue)
        {
            // First, remove any tasks that are determined to be conflicting.
            currentQueue.ForEach(l => l.RemoveAll(newTask.RemoveOther));
            currentQueue.RemoveAll(l => l.Count == 0);

            // Now, find the highest index possible that doesn't violate the rules.
            var maxIndex = currentQueue.Count - 1;
            for (var i = currentQueue.Count - 1; i >= 0; i--)
            {
                var group = currentQueue[i];
                if (group.Any(t => newTask.InvalidAfterTypes.Contains(t.GetType())))
                {
                    maxIndex = i - 1;
                }
            }
            // Now, find the lowest index possible that doesn't violate the rules.
            var minIndex = 0;
            for (var i = 0; i < currentQueue.Count; i++)
            {
                var group = currentQueue[i];
                if (group.Any(t => newTask.InvalidBeforeTypes.Contains(t.GetType())))
                {
                    minIndex = i + 1;
                }
            }

            // Now insert, if no parallel conflicts
            var inserted = false;
            if (minIndex == currentQueue.Count && maxIndex == currentQueue.Count - 1) // New group at the tail
            {
                currentQueue.Add(new List<Task> { newTask });
                inserted = true;
            }
            else if (maxIndex == -1 && minIndex == 0) // New group at the head
            {
                currentQueue.Insert(0, new List<Task> { newTask });   
                inserted = true;
            }
            else if (minIndex <= maxIndex) // Group in the middle
            {
                foreach (var grp in currentQueue.Where(grp => !grp.Any(t => newTask.InvalidDuringTypes.Contains(t.GetType()))))
                {
                    grp.Add(newTask);
                    inserted = true;
                    break;
                }
            }
            else if (minIndex == maxIndex + 1) // New group in the middle
            {
                currentQueue.Insert(minIndex, new List<Task> { newTask });
                inserted = true;
            }

            if (!inserted) // Queue cannot accept this task at this time
            {
                Logger.LogWarning("The queue cannot accept the task " + newTask + " at this time.");
                throw new InvalidQueueStateException(newTask);
            }
        }
    }
}
