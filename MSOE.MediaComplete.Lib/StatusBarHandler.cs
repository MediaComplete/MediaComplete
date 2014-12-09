using System;
using System.Windows.Threading;

namespace MSOE.MediaComplete.Lib
{
    public class StatusBarHandler
    {
        private readonly DispatcherTimer _timer;
        private static StatusBarHandler _instance;

        public static StatusBarHandler Instance
        {
            get { return _instance ?? (_instance = new StatusBarHandler()); }
        }

        public StatusBarHandler()
        {
            _timer = new DispatcherTimer {IsEnabled = false};
            _timer.Tick += OnTimerFinished;
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
            _timer.Interval = TimeSpan.FromMinutes(30);
            _timer.Start();

        }
    }
}
