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
        [Description("Album")]
        Album,
        [Description("Artist")]
        Artist,
        [Description("Year")]
        Year,
        [Description("Supporting Artist")]
        SupportingArtist,
        [Description("Album Art")]
        AlbumArt,
        [Description("Track Number")]
        TrackNumber,
        [Description("Song Title")]
        SongTitle,
        [Description("Genre")]
        Genre,
        [Description("Rating")]
        Rating
    }
}
