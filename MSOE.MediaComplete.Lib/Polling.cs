using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Class used to set up a timed interval to poll a directory for files, and trigger an event when they 
    /// </summary>
    public class Polling
    {
        private readonly Timer _timer;
        public double TimeInMinutes { get; set; }
        private static Polling _instance;

        public delegate void InboxFilesHandler(IEnumerable<FileInfo> files);
        public static event InboxFilesHandler InboxFilesDetected = delegate {};

        /// <summary>
        /// private constructor to build out a timer object and subscribe to its events
        /// </summary>
        private Polling()
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimerFinished;
            SettingWrapper.RaiseSettingEvent += OnSettingChanged;
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
        /// gets the instance of the singleton Polling
        /// </summary>
        public static Polling Instance
        {
            get { return _instance ?? (_instance = new Polling()); }
        }

        /// <summary>
        /// sets up the new polling interval and change of directory to watch
        /// </summary>
        public void OnSettingChanged()
        {
            var timeInMilliseconds = TimeSpan.FromMinutes(SettingWrapper.GetPollingTime()).TotalMilliseconds;
            _timer.Enabled = false;
            _timer.Interval = timeInMilliseconds;
            _timer.Enabled = SettingWrapper.GetIsPolling();
        }

        /// <summary>
        /// resets the interval to start at the current time
        /// </summary>
        public void Reset()
        {
            _timer.Interval = _timer.Interval;//resarts the interval now with the same interval as before
        }

        /// <summary>
        /// checks whether files exist in the inbox to check fires the event if they do, passing the IEnumerable of files to the event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnTimerFinished(Object sender, ElapsedEventArgs args)
        {
            var inbox = new DirectoryInfo(SettingWrapper.GetInboxDir());
            var files = inbox.EnumerateFiles("*").GetMusicFiles();
            var fileInfos = files as FileInfo[] ?? files.ToArray();
            if(fileInfos.Any())
            {
                InboxFilesDetected(fileInfos);
            }
        }
    }
}
