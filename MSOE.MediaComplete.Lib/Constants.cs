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
        /// Provides shorthand access to Path.DirectorySeparatorChar as a string
        /// </summary>
        public static readonly string PathSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// extension for mp3 files
        /// </summary>
        public const string Mp3FileExtension = ".mp3";

        /// <summary>
        /// extension for WMA files
        /// </summary>
        public const string WmaFileExtension = ".wma";

        /// <summary>
        /// extension for WAV files
        /// </summary>
        public const string WavFileExtension = ".wav";

        /// <summary>
        /// Used to filter and create playlist files
        /// </summary>
        public static readonly IReadOnlyList<string> PlaylistFileExtensions = new List<string> { ".m3u" }.AsReadOnly();

        /// <summary>
        /// User to filter music files
        /// </summary>
        public static readonly List<string> MusicFileExtensions = new List<string>
        {
            Mp3FileExtension, 
            WmaFileExtension, 
            //WavFileExtension TODO: for when supporting WAV files
        };

        /// <summary>
        /// The file dialog's wild card for extensions
        /// </summary>
	    public const string Wildcard = "*";
    }
}
