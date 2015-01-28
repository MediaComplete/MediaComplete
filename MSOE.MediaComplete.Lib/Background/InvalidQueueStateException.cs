using System;

namespace MSOE.MediaComplete.Lib.Background
{
    /// <summary>
    /// Thrown when the Queue is given a task that cannot be safely added anywhere 
    /// in the queue. The user should wait and try again later. More likely, however, 
    /// this is occuring as a result of conflicting dependencies between tasks 
    /// (developer's fault)
    /// </summary>
    public class InvalidQueueStateException : Exception
    {
        /// <summary>
        /// Captures the task that caused the issue
        /// </summary>
        public Task Task { get; set; }

        /// <summary>
        /// Constructs the exception with the given provoking task
        /// </summary>
        /// <param name="t">The task that caused the issue</param>
        public InvalidQueueStateException(Task t)
            : base("Queue cannot accept this task at this time")
        {
            Task = t;
        }
    }
}
