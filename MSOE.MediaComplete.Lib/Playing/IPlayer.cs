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
        /// <param name="newValue"></param>
        void ChangeVolume(double newValue);

        //void Seek();
        /// <summary>
        /// fires when the playback has ended, ie. hitting the end of the file
        /// </summary>
        event EventHandler PlaybackEnded;

        /// <summary>
        /// shows the state of the IPlayer
        /// </summary>
        PlaybackState PlaybackState { get; }
    }
}