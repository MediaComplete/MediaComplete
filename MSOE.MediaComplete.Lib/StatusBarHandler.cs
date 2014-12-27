using System;
using System.Timers;

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
            ChangeStatusBarMessage(null, StatusIcon.None);
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
        public delegate void StatusBarChanged(string format, string messageKey, StatusIcon icon, params object[] extraArgs);

        public event StatusBarChanged RaiseStatusBarEvent = delegate { };

        public void ChangeStatusBarMessage(string message, StatusIcon icon)
        {
            RaiseStatusBarEvent("{0}", message, icon);
        }

        public void ChangeStatusBarMessage(string format, string messageKey, StatusIcon icon, params object[] extraArgs)
        {
            SetTimer();
            RaiseStatusBarEvent(format, messageKey, icon, extraArgs);
        }

        private void SetTimer()
        {
            _timer.Stop();
            _timer.Interval = 1000 * 60 * Interval;
            _timer.Start();
        }
    }
}
