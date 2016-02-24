using System;
using System.Collections.Generic;
using System.Threading;
using Sys = System.Threading.Tasks;

namespace MediaComplete.Lib.Background
{
    /// <summary>
    /// This abstract class represents a background queue task. 
    /// </summary>
    public abstract class Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        protected Task()
        {
            Lock = new SemaphoreSlim(1, 1);
            Lock.Wait();
        }

        /// <summary>
        /// Delegate definition for the <see cref="Update"/> event.
        /// </summary>
        /// <param name="data">The task itself, for accessing relevant information</param>
        public delegate void UpdateHandler(Task data);
        
        /// <summary>
        /// Creators of a task may subscribe to it to receive updates when the task chooses to trigger them.
        /// </summary>
        public event UpdateHandler Update = delegate { };

        /// <summary>
        /// Called by the task when it has a new status, so logs, status bar, etc. can be updated by the queue.
        /// </summary>
        /// <param name="data">The data (the <see cref="Task"/> itself)</param>
        protected void TriggerUpdate(Task data)
        {
            Update(data);
        }

        /// <summary>
        /// Is fired on task completion
        /// </summary>
        public event UpdateHandler Done = delegate { };

        /// <summary>
        /// Called by the task when it has completed.
        /// </summary>
        /// <param name="data">The data (the <see cref="Task"/> itself)</param>
        protected void TriggerDone(Task data)
        {
            data.PercentComplete = 1;
            TriggerUpdate(data);
            Lock.Release();
            Done(data);
        }

        /// <summary>
        /// Read-only semaphore. Locks on construction, and releases when "Do" 
        /// completes (successfully or unsuccessfully)
        /// </summary>
        public SemaphoreSlim Lock { get; private set; }
        /// <summary>
        /// The id of this task, as assigned by the work queue.
        /// </summary>
        public int Id { get; set; }
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
        /// Contains the most recent exception encountered while running the task.
        /// </summary>
        public Exception Error { get; set; }

#region Abstract

        /// <summary>
        /// Contains any subclass types that cannot appear before this task in the execution queue. 
        /// Used by <see cref="TaskAdder.ResolveConflicts"/> to re-order the queue after adding this task.
        /// </summary>
        public abstract IReadOnlyCollection<Type> InvalidBeforeTypes { get; }
        /// <summary>
        /// Contains any subclass types that cannot appear after this task in the execution queue. 
        /// Used by <see cref="TaskAdder.ResolveConflicts"/> to re-order the queue after adding this task.
        /// </summary>
        public abstract IReadOnlyCollection<Type> InvalidAfterTypes { get; }
        /// <summary>
        /// Contains any subclass types that cannot appear in the same parallel block in the execution queue. 
        /// Used by <see cref="TaskAdder.ResolveConflicts"/> to re-order the queue after adding this task.
        /// </summary>
        public abstract IReadOnlyCollection<Type> InvalidDuringTypes { get; }
        /// <summary>
        /// Used by subclasses to determine if the task should be removed before adding this one.
        /// </summary>
        /// <param name="t">The other task to consider</param>
        /// <returns>true if t should be removed, false otherwise</returns>
        public abstract bool RemoveOther(Task t);

        /// <summary>
        /// Performs the action of this task, asynchronously. 
        /// </summary>
        /// <param name="i">The index of this task, as assigned by the queue when it's started</param>
        /// <returns>An asynchronous Task</returns>
        public abstract void Do(int i);

#endregion
    }
}
