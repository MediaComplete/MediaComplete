using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Provides functions for identifying a song
    /// </summary>
    public class MusicIdentifier
    {
        private const int Freq = 8000;
        private const int SampleSeconds = 10;

        /// <summary>
        /// Controls how music data is read in for identification
        /// </summary>
        public IAudioReader AudioReader { get; private set; }

        /// <summary>
        /// Controls how music data is read in for identification
        /// </summary>
        public IAudioIdentifier AudioIdentifier { get; private set; }

        /// <summary>
        /// Controls where additional metadata is retreived from after identifying a song.
        /// </summary>
        public IMetadataRetriever MetadataRetriever { get; private set; }

        /// <summary>
        /// Controls how files are accessed and edited
        /// </summary>
        public IFileMover FileMover { get; set; }

        /// <summary>
        /// Defaults to using FFMPEG for audio parsing, Doreso for identification, and our FileMover for file access. 
        /// Spotify will be lazily constructed later for accessing additional metadata details, since it is not 
        /// always necessary and needs asynchronous construction
        /// </summary>
        public MusicIdentifier()
        {
            AudioReader = new FfmpegAudioReader();
            AudioIdentifier = new DoresoIdentifier();
            FileMover = Lib.FileMover.Instance;
        }

        /// <summary>
        /// Use the specified services for identification.
        /// </summary>
        /// <param name="reader">Audio reader for parsing in audio data</param>
        /// <param name="identifier">Audio identifer for fingerprinting songs</param>
        /// <param name="metadata">Metadata retreiver for finding additional metadata details.</param>
        /// <param name="fileMover">Controls how files are accessed</param>
        public MusicIdentifier(IAudioReader reader, IAudioIdentifier identifier, IMetadataRetriever metadata, IFileMover fileMover)
        {
            AudioReader = reader;
            MetadataRetriever = metadata;
            AudioIdentifier = identifier;
            FileMover = fileMover;
        }

        /// <summary>
        /// Identify a song; restoring its metadata based on the audio data
        /// </summary>
        /// <param name="fileMover">Service for accessing the song</param>
        /// <param name="filename">The name of the target song</param>
        /// <returns></returns>
        public async Task IdentifySongAsync(FileMover fileMover, string filename)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Started",
                StatusBarHandler.StatusIcon.Working);

            // Check the file
            if (!fileMover.FileExists(filename))
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException",
                    StatusBarHandler.StatusIcon.Error);
                return;
            }

            // Read in audio data
            var audioData = await AudioReader.ReadBytesAsync(filename, Freq, SampleSeconds);
            if (audioData == null)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException",
                    StatusBarHandler.StatusIcon.Error);
                return;
            }

            Metadata data;
            try
            {
                data = await AudioIdentifier.IdentifyAsync(audioData);
            }
            catch (IdentificationException) // We've exceeded our API rate limit. Tell the user to try again later.
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Warning-RateLimit",
                    StatusBarHandler.StatusIcon.Warning);
                // TODO MC-45 Any other ID tasks in the queue should be cancelled somehow
                return;
            }

            if (data.Title == null) // No match. Tell the user they'll have to do it themselves.
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("{0}: {1}", "MusicIdentification-Warning-NoMatch",
                    StatusBarHandler.StatusIcon.Warning, filename);
                return;
            }

             // Found a match; populate the file
            if (MetadataRetriever == null)
                MetadataRetriever = await SpotifyMetadataRetriever.GetInstanceAsync();
            await MetadataRetriever.GetMetadataAsync(data);

            UpdateFileWithMetadata(data, filename);

            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Success",
                StatusBarHandler.StatusIcon.Success);
            
        }

        /// <summary>
        /// Copies over attributes from a Metadata object to the specified file's taglib. 
        /// Null attributes on the Metadata object are skipped, so there are no destructive overwrites.
        /// </summary>
        /// <param name="data">The new metadata</param>
        /// <param name="filename">The file to update</param>
        private void UpdateFileWithMetadata(Metadata data, string filename)
        {
            var metadata = FileMover.CreateTaglibFile(filename);

            if (data.Title != null)
            {
                metadata.SetAttribute(MetaAttribute.SongTitle, data.Title);
            }
            if (data.Album != null)
            {
                metadata.SetAttribute(MetaAttribute.Album, data.Album);
            }
            if (data.TrackNumber.HasValue)
            {
                metadata.SetAttribute(MetaAttribute.Year, data.TrackNumber.Value);
            }
            if (data.SupportingArtists != null)
            {
                metadata.SetAttribute(MetaAttribute.SupportingArtist, data.SupportingArtists);
            }
            if (data.AlbumArtists != null)
            {
                metadata.SetAttribute(MetaAttribute.Artist, data.AlbumArtists);
            }
            if (data.Rating.HasValue)
            {
                metadata.SetAttribute(MetaAttribute.Year, data.Rating.Value);
            }
            if (data.AlbumArt != null)
            {
                metadata.SetAttribute(MetaAttribute.AlbumArt, data.AlbumArt);
            }
            if (data.Genre != null)
            {
                metadata.SetAttribute(MetaAttribute.Genre, data.Genre);
            }
            if (data.Year.HasValue)
            {
                metadata.SetAttribute(MetaAttribute.Year, data.Year.Value);
            }
        }
    }
}