using System;
using MSOE.MediaComplete.Lib.Library.DataSource;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// Interface for an audio player library
    /// </summary>
    public interface INAudioWrapper
    {
        /// <summary>
        /// Prepare to play a song
        /// </summary>
        /// <param name="localSong">The song file</param>
        /// <param name="handler">The callback for when the song events</param>
        /// <param name="currentVolume">The current volume</param>
        void Setup(LocalSong localSong, EventHandler<StoppedEventArgs> handler, double currentVolume);

        /// <summary>
        /// Play the song
        /// </summary>
        /// <returns>A state confirmation</returns>
        PlaybackState Play();

        /// <summary>
        /// Pauses the song
        /// </summary>
        /// <returns>A state confirmation</returns>
        PlaybackState Pause();

        /// <summary>
        /// Stops the song
        /// </summary>
        /// <returns>A state confirmation</returns>
        PlaybackState Stop();

        /// <summary>
        /// Seeks to the specified time
        /// </summary>
        /// <param name="timeToSeekTo">The time to seek to.</param>
        void Seek(TimeSpan timeToSeekTo);

        /// <summary>
        /// Changes the volume.
        /// </summary>
        /// <param name="sliderVolume">The slider volume.</param>
        void ChangeVolume(double sliderVolume);

        /// <summary>
        /// Gets the total time on the currently playing song
        /// </summary>
        /// <value>
        /// The total time.
        /// </value>
        TimeSpan TotalTime { get; }

        /// <summary>
        /// Gets the current time on the currently playing song
        /// </summary>
        /// <value>
        /// The current time.
        /// </value>
        TimeSpan CurrentTime { get; }
    }
}