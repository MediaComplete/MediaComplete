using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
	///<summary>
	/// Compiled constants used throughout the application. This should only be used for values we don't want the user to be able to edit.
	///</summary>
    public class Constants
    {
		///<summary>
		/// The file pattern used to filter for music files. Currently only includes the MP3 file format.
		///</summary>
		public static readonly string MusicFilePattern = "*.MP3";
    }
}
