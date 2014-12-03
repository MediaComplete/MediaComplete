using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
    public class StatusBarHandler
    {
        public enum StatusIcon 
        {
            None,
            Working,
            Info,
            Warning,
            Error,
            Success
        }
        public delegate void StatusBarChanged();

        public static string Message { get; set; }
        public static StatusIcon Icon { get; set; }

        public static event StatusBarChanged RaiseStatusBarEvent = delegate { };

        public static void ChangeStatusBarMessage(string message, StatusIcon icon)
        {
            StatusBarHandler.Message = message;
            StatusBarHandler.Icon = icon;
            RaiseStatusBarEvent.Invoke();
        }
    }
}
