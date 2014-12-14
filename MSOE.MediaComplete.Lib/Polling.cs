using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace MSOE.MediaComplete.Lib
{
    public class Polling
    {
        private readonly Timer _timer;
        public double TimeInMinutes { get; set; }
        public string InboxDir { get; set; }
        private static Polling _instance;

        public delegate void InboxFilesHandler(IEnumerable<FileInfo> files);
        public static event InboxFilesHandler InboxFilesDetected = delegate {};


        private Polling()
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimerFinished;
        }

        public void Start()
        {
            var timeInMilliseconds = TimeSpan.FromMinutes(TimeInMinutes).TotalMilliseconds;
            _timer.Interval = timeInMilliseconds;
            _timer.Enabled = true;
            Console.WriteLine("Timer Started");
        }

        public static Polling Instance
        {
            get { return _instance ?? (_instance = new Polling()); }
        }

        public void PollingChanged(double newTimeInMinutes, string dir)
        {
            InboxDir = dir;
            var timeInMilliseconds = TimeSpan.FromMinutes(newTimeInMinutes).TotalMilliseconds;
            _timer.Enabled = false;
            _timer.Interval = timeInMilliseconds;
            _timer.Enabled = true;
        }

        [STAThread]
        private void OnTimerFinished(Object sender, ElapsedEventArgs args)
        {
            var inbox = new DirectoryInfo(SettingWrapper.GetInboxDir());
            var files = inbox.EnumerateFiles("*.mp3");
            // ReSharper disable PossibleMultipleEnumeration
            if(files.Any())
            {
                InboxFilesDetected(files);
            }
            // ReSharper restore PossibleMultipleEnumeration
            
            //await Task.Run(() => Importer.Instance.ImportDirectory(inboxDir, false));
        }
    }
}
