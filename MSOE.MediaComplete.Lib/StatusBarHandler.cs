
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
        public delegate void StatusBarChanged(string message, StatusIcon icon);

        public static event StatusBarChanged RaiseStatusBarEvent = delegate { };

        public static void ChangeStatusBarMessage(string message, StatusIcon icon)
        {
            RaiseStatusBarEvent(message, icon);
        }
    }
}
