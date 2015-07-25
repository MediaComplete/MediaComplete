using System;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// interface to define the requirements of the player object
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Attempt to play from the Now Playing queue, throw CorruptFileException when files are corrupt or not a playable type
        /// </summary>
        void Play();

        /// <summary>
        /// pause the currently playing song
        /// </summary>
        void Pause();

        /// <summary>
        /// resume a currently paused song
        /// </summary>
        void Resume();

        /// <summary>
        /// stop the currently playing song
        /// </summary>
        void Stop();

        /// <summary>
        /// Changes the volume of the currently playing song
        /// </summary>
        /// <param name="newValue">The new volume level</param>
        void ChangeVolume(double newValue);

        /// <summary>
        /// seeks to the given time within a song
        /// </summary>
        /// <param name="timeToSeekTo">The target time to seek to</param>
        void Seek(TimeSpan timeToSeekTo);
        
        /// <summary>
        /// fires when the playback has ended, i.e. hitting the end of the file
        /// </summary>
        event EventHandler PlaybackEnded;

        /// <summary>
        /// shows the state of the IPlayer
        /// </summary>
        PlaybackState PlaybackState { get; }

        /// <summary>
        /// gets the current time within the currently playing song
        /// </summary>
        TimeSpan CurrentTime { get; }

        /// <summary>
        /// gets the total runtime of the currently playing song
        /// </summary>
        TimeSpan TotalTime { get; }
    }
}