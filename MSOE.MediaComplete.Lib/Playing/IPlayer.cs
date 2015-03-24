using System;
using MSOE.MediaComplete.Lib.Songs;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// interface to define the requirements of the player object
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// attempt to play the song specified, throw CorruptFileException when files are corrupt or not a playable type
        /// </summary>
        /// <param name="song"></param>
        void Play(AbstractSong song);

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