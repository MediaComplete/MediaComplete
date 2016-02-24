using System;
using System.Timers;

namespace MediaComplete.Lib
{
    /// <summary>
    /// Routes messages from the library to the UI.
    /// </summary>
    public class StatusBarHandler
    {
        /// <summary>
        /// Gets or sets the delay before clearing messages.
        /// </summary>
        /// <value>
        /// The time interval.
        /// </value>
        public double Interval { get; set; }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static StatusBarHandler Instance
        {
            get { return _instance ?? (_instance = new StatusBarHandler()); }
        }
        private static StatusBarHandler _instance;

        private readonly Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBarHandler"/> class.
        /// </summary>
        public StatusBarHandler()
        {
            Interval = 30;
            _timer = new Timer();
            _timer.Elapsed += OnTimerFinished;
        }

        private void OnTimerFinished(object sender, EventArgs eventArgs)
        {
            ChangeStatusBarMessage(null, StatusIcon.None);
            _timer.Stop();
        }

        private void SetTimer()
        {
            _timer.Stop();
            _timer.Interval = 1000 * 60 * Interval;
            _timer.Start();
        }

        /// <summary>
        /// Delegate definition for handling a new status bar message
        /// </summary>
        /// <param name="format">The format string of the message</param>
        /// <param name="messageKey">The message key, to look up a base resource message</param>
        /// <param name="icon">The icon/priority/severity of the message</param>
        /// <param name="extraArgs">The extra arguments to roll into the format string</param>
        public delegate void StatusBarChanged(string format, string messageKey, StatusIcon icon, params object[] extraArgs);

        /// <summary>
        /// Occurs when a new message is incoming
        /// </summary>
        public event StatusBarChanged RaiseStatusBarEvent = delegate { };

        /// <summary>
        /// Fires a new status bar message with just a simple message and icon
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="icon">The icon.</param>
        public void ChangeStatusBarMessage(string message, StatusIcon icon)
        {
            ChangeStatusBarMessage("{0}", message, icon);
        }

        /// <summary>
        /// Fires a new status bar message with the full parameters
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="extraArgs">The extra arguments.</param>
        public void ChangeStatusBarMessage(string format, string messageKey, StatusIcon icon, params object[] extraArgs)
        {
            SetTimer();
            RaiseStatusBarEvent(format, messageKey, icon, extraArgs);
        }

        /// <summary>
        /// Icon to represent the status symbol to display. 
        /// </summary>
        public enum StatusIcon
        {
            // The order of this enumeration should be preserved, as it is used as a priority indicator in Queue.cs

            /// <summary>
            /// Represents "no message"
            /// </summary>
            None,

            /// <summary>
            /// A "work in progress" icon
            /// </summary>
            Working,

            /// <summary>
            /// Represents an informational message
            /// </summary>
            Info,

            /// <summary>
            /// Represents a success message
            /// </summary>
            Success,

            /// <summary>
            /// Represents a warning message
            /// </summary>
            Warning,

            /// <summary>
            /// Represents an error message
            /// </summary>
            Error
        }
    }
}
