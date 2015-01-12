using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Background;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Task runtime for a sorter instance. Used internally.
    /// </summary>
    public class SortingTask : Task
    {
        public Sorter Sorter { get; set; }

        /// <summary>
        /// Creates a sorter task that uses the passed in Sorter instance to perform the operation.
        /// </summary>
        /// <param name="sorter"></param>
        public SortingTask(Sorter sorter)
        {
            Sorter = sorter;
            IsDone = false;
        }

        /// <summary>
        /// Adds this sorting task into the given queue, after any pending Import or Identify tasks. 
        /// If there are any other sorts queued up, they are removed.
        /// </summary>
        /// <param name="currentQueue"></param>
        public override void ResolveConflicts(List<List<Task>> currentQueue)
        {
            var maxIndex = currentQueue.Count;
            var foundIndex = false;
            for (var i = currentQueue.Count - 1; i >= 0; i--)
            {
                var group = currentQueue[i];

                group.RemoveAll(t => t is SortingTask);

                if (group.Count == 0)
                {
                    currentQueue.Remove(group);
                    maxIndex--;
                }
                else if (!foundIndex && group.Any(t => t is ImportTask || t is IdentifierTask))
                {
                    maxIndex = i + 1;
                    foundIndex = true;
                }
            }
            // Insert into the group after the last group with imports or identifies
            if (maxIndex >= currentQueue.Count)
            {
                currentQueue.Add(new List<Task>());
            }
            currentQueue[maxIndex].Add(this);
        }

        /// <summary>
        /// Performs the sort, calculating the necessary actions first, if necessary.
        /// </summary>
        /// <param name="i">The task identifier</param>
        /// <returns>An awaitable task</returns>
        public async override Sys.Task Do(int i)
        {
            Index = i;
            Message = "Sorting-InProgress";

            if (Sorter.Actions.Count == 0)
            {
                await Sorter.CalculateActions();
            }

            var counter = 0;
            var max = (Sorter.Actions.Count > 100 ? Sorter.Actions.Count / 100 : 1);
            var total = 0;
            foreach (var action in Sorter.Actions)
            {
                try
                {
                    action.Do();
                }
                catch (IOException e)
                {
                    Error = e;
                }
                total++;

                if (counter++ >= max)
                {
                    counter = 0;
                    PercentComplete = ((double)total)/Sorter.Actions.Count;
                    TriggerUpdate(this);
                }
            }
            Sorter.Settings.Root.ScrubEmptyDirectories();
            IsDone = true;
        }
    }
}
