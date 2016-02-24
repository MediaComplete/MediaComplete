using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaComplete.Lib.Library.DataSource;
using TagLib;

namespace MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Represents an error in the identification process
    /// </summary>
    public class IdentificationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentificationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IdentificationException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentificationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The nested exception.</param>
        public IdentificationException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }

    /// <summary>
    /// Interface for an audio file parser. Used to retrieve audio data for identification.
    /// </summary>
    public interface IAudioReader
    {
        /// <summary>
        /// Reads in audio bytes.
        /// </summary>
        /// <param name="file">The music file to read from. The encoding/extension do not matter.</param>
        /// <param name="frequency">The desired frequency of sampling.</param>
        /// <param name="sampleSeconds">The number of seconds of audio</param>
        /// <returns>A byte array of WAV data</returns>
        Task<byte[]> ReadBytesAsync(LocalSong file, int frequency, uint sampleSeconds);
    }

    /// <summary>
    /// Identifies music based on an array of WAV bytes
    /// </summary>
    public interface IAudioIdentifier
    {
        /// <summary>
        /// Returns a partially constructed Metadata object based on the audio WAV data.
        /// </summary>
        /// <param name="audioBytes">A sample of audio data</param>
        /// <param name="file">The song object to fill in metadata on based on identification</param>
        /// <returns>A partially populated Metadata object</returns>
        Task IdentifyAsync(byte[] audioBytes, LocalSong file);
    }

    /// <summary>
    /// Finishes populating a Metadata object
    /// </summary>
    public interface IMetadataRetriever
    {
        /// <summary>
        /// Locates more metadata to flesh out the passed in metadata object
        /// </summary>
        /// <param name="file">Assumed to already contain information that can be used to look up more.</param>
        /// <returns>An awaitable</returns>
        Task GetMetadataAsync(LocalSong file);
    }

    /// <summary>
    /// Contains additional track metadata from Spotify to flesh out the ID3 tags with
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets or sets the song title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the song's album.
        /// </summary>
        /// <value>
        /// The album.
        /// </value>
        public string Album { get; set; }

        /// <summary>
        /// Gets or sets the album's release year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public uint? Year { get; set; }

        /// <summary>
        /// Gets or sets the album artists.
        /// </summary>
        /// <value>
        /// The album artists.
        /// </value>
        public IEnumerable<string> AlbumArtists { get; set; }

        /// <summary>
        /// Gets or sets the supporting artists.
        /// </summary>
        /// <value>
        /// The supporting artists.
        /// </value>
        public IEnumerable<string> SupportingArtists { get; set; }

        /// <summary>
        /// Gets or sets the album art.
        /// </summary>
        /// <value>
        /// The album art.
        /// </value>
        public IPicture AlbumArt { get; set; }

        /// <summary>
        /// Gets or sets the track number.
        /// </summary>
        /// <value>
        /// The track number.
        /// </value>
        public uint? TrackNumber { get; set; }

        /// <summary>
        /// Gets or sets the genre.
        /// </summary>
        /// <value>
        /// The genre.
        /// </value>
        public string Genre { get; set; }

        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        /// <value>
        /// The rating.
        /// </value>
        public uint? Rating { get; set; }
    }
}
