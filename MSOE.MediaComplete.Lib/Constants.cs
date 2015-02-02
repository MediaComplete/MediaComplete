using System.Collections.Generic;

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
	    public const string PathSeparator = "\\";

        /// <summary>
        /// extension for mp3 files
        /// </summary>
        public const string Mp3FileExtension = ".mp3";

        /// <summary>
        /// extension for wma files
        /// </summary>
        public const string WmaFileExtension = ".wma";

        /// <summary>
        /// extension for wav files
        /// </summary>
        public const string WavFileExtension = ".wav";

        /// <summary>
        /// User to filter music files
        /// </summary>
        public static readonly List<string> MusicFileExtensions = new List<string>
        {
            Mp3FileExtension, 
            WmaFileExtension, 
            //WavFileExtension TODO: for when supporting wav files
        };

        /// <summary>
        /// The file dialog's wild card for extensions
        /// </summary>
	    public const string Wildcard = "*";
    }
}
