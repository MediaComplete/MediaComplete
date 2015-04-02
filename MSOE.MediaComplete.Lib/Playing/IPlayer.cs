using System;
using System.IO;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// interface to define the requirements of the player object
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// attempt to play the file specified, throw CorruptFileException when files are corrupt or not a playable type
        /// </summary>
        /// <param name="file"></param>
        void Play(FileInfo file);

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
        /// <param name="newValue"></param>
        void ChangeVolume(double newValue);

        /// <summary>
        /// seeks to the given time within a song
        /// </summary>
        /// <param name="timeToSeekTo"></param>
        void Seek(TimeSpan timeToSeekTo);
        
        /// <summary>
        /// fires when the playback has ended, ie. hitting the end of the file
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