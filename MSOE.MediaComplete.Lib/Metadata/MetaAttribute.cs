using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MSOE.MediaComplete.Lib.Metadata
{
	/// <summary>
	/// An enum representing the ID3 metadata values we care about.
	/// </summary>
    ///
    [ComVisible(true)]
    [Flags]
    [TypeConverter(typeof(KeysConverter))]
    public enum MetaAttribute
    {
        Album,
        Artist,
        Year,
        SupportingArtist,
        AlbumArt,
        TrackNumber,
        SongTitle,
        Genre,
        Rating
    }
}
