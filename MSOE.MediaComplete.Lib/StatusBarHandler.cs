using System;
using System.Timers;
using System.Windows.Threading;

namespace MSOE.MediaComplete.Lib
{
    public class StatusBarHandler
    {
        private readonly Timer _timer;
        private static StatusBarHandler _instance;
        public double Interval { get; set; }

        public static StatusBarHandler Instance
        {
            get { return _instance ?? (_instance = new StatusBarHandler()); }
        }

        public StatusBarHandler()
        {
            Interval = 30;
            _timer = new Timer();
            _timer.Elapsed += OnTimerFinished;
        }

        private void OnTimerFinished(object sender, EventArgs eventArgs)
        {
            ChangeStatusBarMessage("", StatusIcon.None);
            _timer.Stop();
        }

        public enum StatusIcon 
        {
            None,
            Working,
            Info,
            Warning,
            Error,
            Success
        }
        public delegate void StatusBarChanged(string message, StatusIcon icon);

        public event StatusBarChanged RaiseStatusBarEvent = delegate { };

        public void ChangeStatusBarMessage(string message, StatusIcon icon)
        {
            SetTimer();
            RaiseStatusBarEvent(message, icon);
        }

        private void SetTimer()
        {
            _timer.Stop();
            _timer.Interval = 1000 * 60 * .1;
            _timer.Start();

        }
    }
}
