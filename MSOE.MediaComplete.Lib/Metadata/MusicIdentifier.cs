using System.Threading.Tasks;
﻿using MSOE.MediaComplete.Lib.Files;

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
        public IFileManager FileManager { get; set; }

        /// <summary>
        /// Defaults to using FFMPEG for audio parsing, Doreso for identification, and our FileMover for file access. 
        /// Spotify will be lazily constructed later for accessing additional metadata details, since it is not 
        /// always necessary and needs asynchronous construction
        /// </summary>
        public MusicIdentifier()
        {
            AudioReader = new FfmpegAudioReader();
            AudioIdentifier = new DoresoIdentifier();
            FileManager = Files.FileManager.Instance;
        }

        /// <summary>
        /// Use the specified services for identification.
        /// </summary>
        /// <param name="reader">Audio reader for parsing in audio data</param>
        /// <param name="identifier">Audio identifer for fingerprinting songs</param>
        /// <param name="metadata">Metadata retreiver for finding additional metadata details.</param>
        /// <param name="fileManager">Controls how files are accessed</param>
        public MusicIdentifier(IAudioReader reader, IAudioIdentifier identifier, IMetadataRetriever metadata, IFileManager fileManager)
        {
            AudioReader = reader;
            MetadataRetriever = metadata;
            AudioIdentifier = identifier;
            FileManager = fileManager;
        }

        /// <summary>
        /// Identify a song; restoring its metadata based on the audio data
        /// </summary>
        /// <param name="fileMover">Service for accessing the song</param>
        /// <param name="file">The target song</param>
        /// <returns></returns>
        public async Task IdentifySongAsync(IFileManager fileMover, LocalSong file)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Started",
                StatusBarHandler.StatusIcon.Working);

            if (!fileMover.FileExists(file.SongPath))
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException",
                    StatusBarHandler.StatusIcon.Error);
                return;
            }

            // Read in audio data
            var audioData = await AudioReader.ReadBytesAsync(file, Freq, SampleSeconds);
            if (audioData == null)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException",
                    StatusBarHandler.StatusIcon.Error);
                return;
            }

            try
            {
                await AudioIdentifier.IdentifyAsync(audioData, file);
            }
            catch (IdentificationException) // We've exceeded our API rate limit. Tell the user to try again later.
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Warning-RateLimit",
                    StatusBarHandler.StatusIcon.Warning);
                // TODO MC-45 Any other ID tasks in the queue should be cancelled somehow
                throw;
            }

            if (file.Title == null) // No match. Tell the user they'll have to do it themselves.
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("{0}: {1}", "MusicIdentification-Warning-NoMatch",
                    StatusBarHandler.StatusIcon.Warning, file.SongPath);
                return;
            }

             // Found a match; populate the file
            if (MetadataRetriever == null)
                MetadataRetriever = await SpotifyMetadataRetriever.GetInstanceAsync();
            await MetadataRetriever.GetMetadataAsync(file);

            // Save whatever we found
            FileManager.SaveSong(file);

            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Success",
                StatusBarHandler.StatusIcon.Success);
            
        }
    }
}