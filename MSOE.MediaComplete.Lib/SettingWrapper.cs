using System;
using MSOE.MediaComplete.Lib.Properties;

namespace MSOE.MediaComplete.Lib
{
    public class SettingWrapper
    {
        private const string HomeDir = "HomeDir";
        private const string InboxDir = "InboxDir";
        private const string PollingTime = "PollingTime";
        private const string IsPolling = "IsPolling";
        private const string ShowInputDialog = "ShowInputDialog";

        public static event SettingsChangedListener RaiseSettingEvent = delegate {};
        public delegate void SettingsChangedListener();

        public static string GetHomeDir()
        {
            return (string)Settings.Default[HomeDir];
        }

        public static void SetHomeDir(string homeDir)
        {
            Settings.Default[HomeDir] = homeDir;
        }

        public static string GetInboxDir()
        {
            return (string)Settings.Default[InboxDir];
        }

        public static void SetInboxDir(string inboxDir)
        {
            Settings.Default[InboxDir] = inboxDir;
        }

        public static double GetPollingTime()
        {
            return Convert.ToDouble(Settings.Default[PollingTime]);
        }

        public static void SetPollingTime(object pollingTime)
        {
            Settings.Default[PollingTime] = pollingTime;
        }

        public static bool GetIsPolling()
        {
            return (bool)Settings.Default[IsPolling];
        }

        public static void SetIsPolling(bool isPolling)
        {
            Settings.Default[IsPolling] = isPolling;
        }

        public static bool GetShowInputDialog()
        {
            return (bool)Settings.Default[ShowInputDialog];
        }

        public static void SetShowInputDialog(bool showInputDialog)
        {
            Settings.Default[ShowInputDialog] = showInputDialog;
        }

        public static void Save()
        {
            Settings.Default.Save();
            RaiseSettingEvent.Invoke();
        }
    }
}