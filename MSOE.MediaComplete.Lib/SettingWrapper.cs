using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Properties;

namespace MSOE.MediaComplete.Lib
{
    /// <summary>
    /// Used to wrap the settings object and encapsulate its use, while allowing ats access in both the Lib project and the UI project
    /// </summary>
    public static class SettingWrapper
    {
        public static event SettingsChangedListener RaiseSettingEvent = delegate {};
        public delegate void SettingsChangedListener();

        /// <summary>
        /// Gets the home directory from the settings
        /// </summary>
        /// <returns>home directory path</returns>
        public static string HomeDir
        {
            get { return (string)Settings.Default["HomeDir"]; }
            set { Settings.Default["HomeDir"] = value.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? value : value + Path.DirectorySeparatorChar; }
        }

        /// <summary>
        /// Gets the music directory from the settings
        /// </summary>
        /// <returns>music directory path</returns>
        public static string MusicDir
        {
            get
            {
                var mDir = HomeDir + Settings.Default["MusicDir"];
                return mDir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? mDir : mDir + Path.DirectorySeparatorChar;
            }
        }
        /// <summary>
        /// Gets the playlist directory from the settings
        /// </summary>
        /// <returns>playlist directory path</returns>
        public static string PlaylistDir
        {
            get
            {
                var pDir = HomeDir + Settings.Default["PlaylistDir"];
                return pDir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? pDir : pDir + Path.DirectorySeparatorChar;
            }
        }
        /// <summary>
        /// gets the inbox directory path
        /// </summary>
        /// <returns>inbox directory path</returns>
        public static string InboxDir
        {
            get { return (string)Settings.Default["InboxDir"]; }
            set { Settings.Default["InboxDir"] = value.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? value : value + Path.DirectorySeparatorChar; }
        }

        /// <summary>
        /// gets the polling time from the settings
        /// </summary>
        /// <returns>the polling interval</returns>
        public static double PollingTime
        {
            get { return Convert.ToDouble(Settings.Default["PollingTime"]); } 
            set { Settings.Default["PollingTime"] = value; }
        }

        /// <summary>
        /// gets the ShouldRemoveOnImport boolean from settings
        /// </summary>
        /// <returns>ShouldRemoveOnImport is true if you will remove the original file on import
        /// false if not desired</returns>
        public static bool ShouldRemoveOnImport
        {
            get { return (bool)Settings.Default["ShouldRemoveOnImport"]; }
            set { Settings.Default["ShouldRemoveOnImport"] = value; }
        }

        /// <summary>ShouldRemoveOnImport
        /// gets the IsPolling boolean from settings
        /// </summary>
        /// <returns>IsPolling is true if polling is desired, false if not desired</returns>
        public static bool IsPolling
        {
            get { return (bool)Settings.Default["IsPolling"]; }
            set { Settings.Default["IsPolling"] = value; }
        }

        /// <summary>
        /// gets whether the application should show the inbox import dialog
        /// </summary>
        /// <returns>true if the dialog is to be shown, false if it should not show</returns>
        public static bool ShowInputDialog
        {
            get { return (bool)Settings.Default["ShowInputDialog"]; }
            set { Settings.Default["ShowInputDialog"] = value; }

        }
        /// <summary>
        /// gets the IsSorting boolean from settings
        /// </summary>
        /// <returns>IsSorting is true if automatically sorting is desired, false if not desired</returns>
        public static bool IsSorting
        {
            get { return (bool)Settings.Default["IsSorting"]; }
            set { Settings.Default["IsSorting"] = value; }
        }

        public static string Layout
        {
            get { return (string)Settings.Default["Layout"]; }
            set { Settings.Default["Layout"] = value; }
        }
        
        /// <summary>
        /// gets the SortOrder list from settings
        /// </summary>
        /// <returns>The order the sort will perform in</returns>
        public static List<MetaAttribute> SortOrder
        {
            get
            {
                var stringList = ((StringCollection)Settings.Default["SortingOrder"]).Cast<string>().ToList();
                var metaAttrList = stringList.Select(x => (MetaAttribute)Enum.Parse(typeof(MetaAttribute), x)).ToList();
                return metaAttrList;
            }
            set
            {
                var collection = new StringCollection();
                collection.AddRange(value.Select(x => x.ToString()).ToArray());
                Settings.Default["SortingOrder"] = collection;
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