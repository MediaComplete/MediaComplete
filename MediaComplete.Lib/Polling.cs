using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using MediaComplete.Lib.Library.DataSource;
using MediaComplete.Lib.Metadata;

namespace MediaComplete.Lib
{
    /// <summary>
    /// Class used to set up a timed interval to poll a directory for files, and trigger an event when they 
    /// </summary>
    public class Polling : IPolling
    {
        /// <summary>
        /// Delegate definition for handling discover files
        /// </summary>
        /// <param name="files">The new files.</param>
        public delegate void InboxFilesHandler(IEnumerable<SongPath> files);

        /// <summary>
        /// Occurs when files are detected
        /// </summary>
        public static event InboxFilesHandler InboxFilesDetected = delegate { };

        /// <summary>
        /// Gets or sets the polling interval in minutes.
        /// </summary>
        /// <value>
        /// The time in minutes.
        /// </value>
        public double TimeInMinutes { get; set; }

        private readonly Timer _timer;

        /// <summary>
        /// constructor to build out a timer object and subscribe to its events
        /// </summary>
        public Polling()
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimerFinished;
            SettingWrapper.RaiseSettingEvent += OnSettingChanged;
            TimeInMinutes = SettingWrapper.PollingTime;
            if (SettingWrapper.IsPolling)
            {
                Start();
            }
        }

        /// <summary>
        /// starts the interval
        /// </summary>
        public void Start()
        {
            var timeInMilliseconds = TimeSpan.FromMinutes(TimeInMinutes).TotalMilliseconds;
            _timer.Interval = timeInMilliseconds;
            _timer.Enabled = true;
        }
        
        /// <summary>
        /// sets up the new polling interval and change of directory to watch
        /// </summary>
        public void OnSettingChanged()
        {
            var timeInMilliseconds = TimeSpan.FromMinutes(SettingWrapper.PollingTime).TotalMilliseconds;
            _timer.Enabled = false;
            _timer.Interval = timeInMilliseconds;
            _timer.Enabled = SettingWrapper.IsPolling;
        }

        /// <summary>
        /// resets the interval to start at the current time
        /// </summary>
        public void Reset()
        {
            _timer.Interval = _timer.Interval; // restarts the interval now with the same interval as before
        }

        /// <summary>
        /// checks whether files exist in the inbox to check fires the event if they do, passing the IEnumerable of files to the event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnTimerFinished(Object sender, ElapsedEventArgs args)
        {
            var inbox = new DirectoryInfo(SettingWrapper.InboxDir);
            if (!inbox.Exists)
            {
                inbox.Create();
            }

            var files = inbox.EnumerateFiles("*",SearchOption.AllDirectories).GetMusicFiles().Select(x => new SongPath(x.FullName));
            var fileInfos = files as SongPath[] ?? files.ToArray();
            if(fileInfos.Any())
            {
                InboxFilesDetected(fileInfos);
            }
        }
    }

    /// <summary>
    /// Interface for a file polling service
    /// </summary>
    public interface IPolling
    {
        /// <summary>
        /// Resets the instance
        /// </summary>
        void Reset();
    }
}
