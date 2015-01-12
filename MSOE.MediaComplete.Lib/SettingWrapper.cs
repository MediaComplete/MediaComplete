using System;
using MSOE.MediaComplete.Lib.Properties;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Used to wrap the settings object and encapsulate its use, while allowing ats access in both the Lib project and the UI project
    /// </summary>
    public class SettingWrapper
    {
        private const string HomeDir = "HomeDir";
        private const string InboxDir = "InboxDir";
        private const string PollingTime = "PollingTime";
        private const string IsPolling = "IsPolling";
        private const string ShowInputDialog = "ShowInputDialog";
        private const string Layout = "Layout";
        private const string IsSorting = "IsSorting";

        public static event SettingsChangedListener RaiseSettingEvent = delegate {};
        public delegate void SettingsChangedListener();

        /// <summary>
        /// Gets the home directory from the settings
        /// </summary>
        /// <returns>home directory path</returns>
        public static string GetHomeDir()
        {
            return (string)Settings.Default[HomeDir];
        }

        /// <summary>
        /// Sets the home directory path
        /// </summary>
        /// <param name="homeDir">path to set</param>
        public static void SetHomeDir(string homeDir)
        {
            Settings.Default[HomeDir] = homeDir;
        }

        /// <summary>
        /// gets the inbox directory path
        /// </summary>
        /// <returns>inbox directory path</returns>
        public static string GetInboxDir()
        {
            return (string)Settings.Default[InboxDir];
        }

        /// <summary>
        /// sets the inbox directory path
        /// </summary>
        /// <param name="inboxDir">path to set</param>
        public static void SetInboxDir(string inboxDir)
        {
            Settings.Default[InboxDir] = inboxDir;
        }

        /// <summary>
        /// gets the polling time from the settings
        /// </summary>
        /// <returns>the polling interval</returns>
        public static double GetPollingTime()
        {
            return Convert.ToDouble(Settings.Default[PollingTime]);
        }

        /// <summary>
        /// sets the polling interval 
        /// </summary>
        /// <param name="pollingTime">interval to set in minutes</param>
        public static void SetPollingTime(object pollingTime)
        {
            Settings.Default[PollingTime] = pollingTime;
        }

        /// <summary>
        /// gets the IsPolling boolean from settings
        /// </summary>
        /// <returns>IsPolling is true if polling is desired, false if not desired</returns>
        public static bool GetIsPolling()
        {
            return (bool)Settings.Default[IsPolling];
        }

        /// <summary>
        /// sets whether the application should poll the inbox location
        /// </summary>
        /// <param name="isPolling">boolean to set</param>
        public static void SetIsPolling(bool isPolling)
        {
            Settings.Default[IsPolling] = isPolling;
        }

        /// <summary>
        /// gets whether the application should show the inbox import dialog
        /// </summary>
        /// <returns>true if the dialog is to be shown, false if it should not show</returns>
        public static bool GetShowInputDialog()
        {
            return (bool)Settings.Default[ShowInputDialog];
        }

        /// <summary>
        /// sets whether the inbox import dialog should be shown
        /// </summary>
        /// <param name="showInputDialog">bool to set</param>
        public static void SetShowInputDialog(bool showInputDialog)
        {
            Settings.Default[ShowInputDialog] = showInputDialog;
        }
        /// <summary>
        /// gets the IsSorting boolean from settings
        /// </summary>
        /// <returns>IsSorting is true if automatically sorting is desired, false if not desired</returns>
        public static bool GetIsSorting()
        {
            return (bool)Settings.Default[IsSorting];
        }

        /// <summary>
        /// sets whether the application should sort automatically
        /// </summary>
        /// <param name="isSorting">boolean to set</param>
        public static void SetIsSorting(bool isSorting)
        {
            Settings.Default[IsSorting] = isSorting;
        }

        /// <summary>
        /// saves the settings
        /// </summary>
        public static void Save()
        {
            Settings.Default.Save();
            RaiseSettingEvent.Invoke();
        }

        public static string GetLayout()
        {
            return (string)Settings.Default[Layout];
        }

        public static void SetLayout(string layout)
        {
            Settings.Default[Layout] = layout;
        }

    }
}