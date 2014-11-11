using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Media3D;
using MSOE.MediaComplete.Lib.Properties;

namespace MSOE.MediaComplete.Lib
{
    public class Polling
    {
        private Timer _timer;
        public double TimeInMinutes { get; set; }
        public string inboxDir { get; set; }
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
            inboxDir = dir;
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
            if(files.Any())
            {
                InboxFilesDetected(files);
            }
            
            //await Task.Run(() => Importer.Instance.ImportDirectory(inboxDir, false));
        }
    }
}
