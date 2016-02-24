using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using MediaComplete.Lib.Logging;
using MediaComplete.Lib.Library.DataSource;
using MediaComplete.Lib.Metadata;
using MediaComplete.Lib.Properties;

namespace MediaComplete.Lib
{
    /// <summary>
    /// Used to wrap the settings object and encapsulate its use, while allowing access in both the Lib project and the UI project
    /// </summary>
    public static class SettingWrapper
    {
        /// <summary>
        /// Occurs when settings are changed.
        /// </summary>
        public static event SettingsChangedListener RaiseSettingEvent = delegate {};

        /// <summary>
        /// Delegate for handling changed settings
        /// </summary>
        public delegate void SettingsChangedListener();

        /// <summary>
        /// Gets the home directory from the settings
        /// </summary>
        /// <returns>home directory path</returns>
        public static DirectoryPath HomeDir
        {
            get
            {
                // If null or if it has been deleted, fall back on previous HomeDirs (AllDirs). Otherwise, create a new one under the user's music dir.
                if (Settings.Default.HomeDir == null || !Directory.Exists(Settings.Default.HomeDir))
                {
                    Settings.Default.HomeDir = AllDirectories.FirstOrDefault(Directory.Exists);
                    if (Settings.Default.HomeDir == null)
                    {
                        Settings.Default.HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
                                                + Path.DirectorySeparatorChar + "Media Complete";
                        Directory.CreateDirectory(Settings.Default.HomeDir);
                    }

                    if (!Settings.Default.HomeDir.EndsWith(Path.DirectorySeparatorChar + "", StringComparison.Ordinal))
                    {
                        Settings.Default.HomeDir += Path.DirectorySeparatorChar;
                    }
                    Settings.Default.Save();
                }
                return new DirectoryPath(Settings.Default.HomeDir);
            }
            set { Settings.Default.HomeDir = value.FullPath; }
        }

        /// <summary>
        /// Gets the music directory from the settings
        /// </summary>
        /// <returns>music directory path</returns>
        public static DirectoryPath MusicDir
        {
            get { return new DirectoryPath(HomeDir.FullPath + Settings.Default.MusicDir + Path.DirectorySeparatorChar); }
        }
        /// <summary>
        /// Gets the playlist directory from the settings
        /// </summary>
        /// <returns>playlist directory path</returns>
        public static DirectoryPath PlaylistDir
        {
            get { return new DirectoryPath(HomeDir.FullPath + Settings.Default.PlaylistDir + Path.DirectorySeparatorChar); }
        }
        /// <summary>
        /// gets the inbox directory path
        /// </summary>
        /// <returns>inbox directory path</returns>
        public static string InboxDir
        {
            get { return Settings.Default.InboxDir; }
            set { Settings.Default.InboxDir = value; }
        }

        /// <summary>
        /// gets the polling time from the settings
        /// </summary>
        /// <returns>the polling interval</returns>
        public static double PollingTime
        {
            get { return Convert.ToDouble(Settings.Default.PollingTime); } 
            set { Settings.Default.PollingTime = value; }
        }

        /// <summary>
        /// gets the ShouldRemoveOnImport boolean from settings
        /// </summary>
        /// <returns>ShouldRemoveOnImport is true if you will remove the original file on import
        /// false if not desired</returns>
        public static bool ShouldRemoveOnImport
        {
            get { return Settings.Default.ShouldRemoveOnImport; }
            set { Settings.Default.ShouldRemoveOnImport = value; }
        }

        /// <summary>ShouldRemoveOnImport
        /// gets the IsPolling boolean from settings
        /// </summary>
        /// <returns>IsPolling is true if polling is desired, false if not desired</returns>
        public static bool IsPolling
        {
            get { return Settings.Default.IsPolling; }
            set { Settings.Default.IsPolling = value; }
        }

        /// <summary>
        /// gets whether the application should show the inbox import dialog
        /// </summary>
        /// <returns>true if the dialog is to be shown, false if it should not show</returns>
        public static bool ShowInputDialog
        {
            get { return Settings.Default.ShowInputDialog; }
            set { Settings.Default.ShowInputDialog = value; }

        }
        /// <summary>
        /// gets the IsSorting boolean from settings
        /// </summary>
        /// <returns>IsSorting is true if automatically sorting is desired, false if not desired</returns>
        public static bool IsSorting
        {
            get { return Settings.Default.IsSorting; }
            set { Settings.Default.IsSorting = value; }
        }

        /// <summary>
        /// Gets or sets the look and feel of the application
        /// </summary>
        /// <value>
        /// The layout.
        /// </value>
        public static string Layout
        {
            get { return Settings.Default.Layout; }
            set { Settings.Default.Layout = value; }
        }
        
        /// <summary>
        /// gets the SortOrder list from settings
        /// </summary>
        /// <returns>The order the sort will perform in</returns>
        public static List<MetaAttribute> SortOrder
        {
            get
            {
                var stringList = (Settings.Default.SortingOrder).Cast<string>().ToList();
                var metaAttrList = stringList.Select(x => (MetaAttribute)Enum.Parse(typeof(MetaAttribute), x)).ToList();
                return metaAttrList;
            }
            set
            {
                var collection = new StringCollection();
                collection.AddRange(value.Select(x => x.ToString()).ToArray());
                Settings.Default.SortingOrder = collection;
            }
            
        }

        /// <summary>
        /// Gets or sets all home directories.
        /// </summary>
        /// <value>
        /// All directories.
        /// </value>
        public static List<string> AllDirectories
        {
            get { return (Settings.Default.AllDirs).Split(';').ToList(); }
            set { Settings.Default.AllDirs = value.Aggregate((x, y) => x + ";" + y); }
        }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public static int LogLevel
        {
            get { return Settings.Default.LogLevel; }
            set
            {
                Settings.Default.LogLevel = value;
                Logger.SetLogLevel(value);
            }
        }

        /// <summary>
        /// saves the settings
        /// </summary>
        public static void Save()
        {
            Settings.Default.Save();
            RaiseSettingEvent.Invoke();
        }
    }
}