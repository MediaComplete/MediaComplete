using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Files;
using TagLib;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Represents an error in the identification process
    /// </summary>
    public class IdentificationException : Exception
    {
        public IdentificationException(string message) : base(message)
        {
            
        }

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
        public string Title { get; set; }
        public string Album { get; set; }
        public uint? Year { get; set; }
        public IEnumerable<string> AlbumArtists { get; set; }
        public IEnumerable<string> SupportingArtists { get; set; }
        public IPicture AlbumArt { get; set; }
        public uint? TrackNumber { get; set; }
        public string Genre { get; set; }
        public uint? Rating { get; set; }
    }
}
