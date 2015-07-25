using System.ComponentModel;

namespace MSOE.MediaComplete.Lib.Metadata
{
	/// <summary>
	/// An enumeration representing the ID3 metadata values we care about.
	/// </summary>
    public enum MetaAttribute
    {
        /// <summary>
        /// The album
        /// </summary>
        [Description("Album")]
        Album,
        /// <summary>
        /// The album artist
        /// </summary>
        [Description("Artist")]
        Artist,
        /// <summary>
        /// The album release year
        /// </summary>
        [Description("Year")]
        Year,
        /// <summary>
        /// The track supporting artist
        /// </summary>
        [Description("Supporting Artist")]
        SupportingArtist,
        /// <summary>
        /// The album art
        /// </summary>
        [Description("Album Art")]
        AlbumArt,
        /// <summary>
        /// The track number
        /// </summary>
        [Description("Track Number")]
        TrackNumber,
        /// <summary>
        /// The song title
        /// </summary>
        [Description("Song Title")]
        SongTitle,
        /// <summary>
        /// The genre
        /// </summary>
        [Description("Genre")]
        Genre,
        /// <summary>
        /// The rating
        /// </summary>
        [Description("Rating")]
        Rating
    }
}
