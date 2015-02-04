using System;
using System.Collections.Generic;
using System.IO;
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
        }

        /// <summary>
        /// Performs the sort, calculating the necessary actions first, if necessary.
        /// </summary>
        /// <param name="i">The task identifier</param>
        /// <returns>An awaitable task</returns>
        public override void Do(int i)
        {
            try
            {
                Id = i;
                Message = "Sorting-InProgress";
                Icon = StatusBarHandler.StatusIcon.Working;

                if (Sorter.Actions.Count == 0)
                {
                    Sys.Task.Run(() => Sorter.CalculateActionsAsync()).Wait();
                }

                var counter = 0;
                var max = (Sorter.Actions.Count > 100 ? Sorter.Actions.Count/100 : 1);
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
                        Message = "Sorting-HadError";
                        Icon = StatusBarHandler.StatusIcon.Error;
                        TriggerUpdate(this);
                    }

                    total++;
                    if (counter++ >= max)
                    {
                        counter = 0;
                        PercentComplete = ((double) total)/Sorter.Actions.Count;
                        TriggerUpdate(this);
                    }
                }
                Sorter.Settings.Root.ScrubEmptyDirectories();

                if (Error == null)
                {
                    Icon = StatusBarHandler.StatusIcon.Success;
                }
            }
            catch (Exception e)
            {
                Message = "Sorting-HadError";
                Icon = StatusBarHandler.StatusIcon.Error;
                Error = e;
            }
            finally
            {
                TriggerDone(this);
            }
        }

        #region Task Overrides
        public override IReadOnlyCollection<Type> InvalidBeforeTypes
        {
            get { return new List<Type> { typeof(IdentifierTask), typeof(ImportTask) }.AsReadOnly(); }
        }

        public override IReadOnlyCollection<Type> InvalidAfterTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        public override IReadOnlyCollection<Type> InvalidDuringTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        public override bool RemoveOther(Task t)
        {
            return t is SortingTask;
        }
        #endregion
    }
}
