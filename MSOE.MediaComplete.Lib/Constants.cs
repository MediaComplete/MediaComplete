namespace MSOE.MediaComplete.Lib
{
	///<summary>
	/// Compiled constants used throughout the application. This should only be used for values we don't want the user to be able to edit.
	///</summary>
    public static class Constants
    {
		///<summary>
		/// The file pattern used to filter for music files. Currently only includes the MP3 file format.
		///</summary>
		public const string MusicFilePattern = "*.MP3";

        /// <summary>
        /// Used to separate human readable file filters from their regex counterparts when opening a dialog
        /// </summary>
	    public const string FileDialogFilterStringSeparator = "|";

        /// <summary>
        /// Used for backslashes in folder paths
        /// </summary>
	    public const string PathSeparator = "\\";
    }
}
