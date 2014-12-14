using System;
using System.Threading.Tasks;
using System.Timers;

namespace MSOE.MediaComplete.Lib
{
    public class Polling
    {
        private Timer _timer;
        public double TimeInMinutes { get; set; }
        public string InboxDir { get; set; }
        private static Polling _instance;

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

        private async void OnTimerFinished(Object sender, ElapsedEventArgs args)
        {
            await Task.Run(() => Importer.Instance.ImportDirectory(InboxDir, false));
        }
    }
}
