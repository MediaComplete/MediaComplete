using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MSOE.MediaComplete.Lib
{
	///<summary>
	/// Compiled constants used throughout the application. This should only be used for values we don't want the user to be able to edit.
	///</summary>
    public static class Constants
    {
        /// <summary>
        /// Used to separate human readable file filters from their regex counterparts when opening a dialog
        /// </summary>
	    public const string FileDialogFilterStringSeparator = "|";

        /// <summary>
        /// Used for backslashes in folder paths
        /// </summary>
        public static readonly string PathSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// User to filter music files
        /// </summary>
        public static readonly IReadOnlyList<string> MusicFileExtensions = new List<string> { ".mp3", ".wma" }.AsReadOnly();

        /// <summary>
        /// Used to filter and create playlist files
        /// </summary>
        public static readonly IReadOnlyList<string> PlaylistFileExtensions = new List<string> { ".m3u" }.AsReadOnly();

        /// <summary>
        /// The file dialog's wild card for extensions
        /// </summary>
	    public const string Wildcard = "*";
    }
}
