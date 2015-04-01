using System;
using System.IO;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// implementation of the IPlayer interface utilizing NAudio
    /// </summary>
    public class Player : IPlayer
    {
        #region singleton stuff
        /// <summary>
        /// backing store for the singleton instance
        /// </summary>
        private static Player _instance;

        /// <summary>
        /// gets the singleton instance of the Player
        /// </summary>
        public static Player Instance
        {
            get { return _instance ?? (_instance = new Player(new NAudioWrapper())); }
        }

        /// <summary>
        /// private constructor to prevent creation of more than one Player instance
        /// </summary>
        internal Player(INAudioWrapper nAudioWrapper)
        {
            _nAudioWrapper = nAudioWrapper;
        }
        #endregion

        #region properties
        /// <summary>
        /// factory used to get the WaveStream
        /// </summary>
        private readonly INAudioWrapper _nAudioWrapper;

        /// <summary>
        /// the state of the player
        /// </summary>
        public PlaybackState PlaybackState { get; private set; }
        #endregion

        #region IPlayer methods
        /// <summary>
        /// sets up the player and plays the file
        /// </summary>
        /// <param name="file">file to play</param>
        public void Play(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");

            Stop();

            _nAudioWrapper.Setup(file, WaveOutOnPlaybackStopped);
            PlaybackState = _nAudioWrapper.Play();
            
        }

        /// <summary>
        /// pauses the current playback
        /// </summary>
        public void Pause()
        {
            PlaybackState = _nAudioWrapper.Pause();
        }

        /// <summary>
        /// resumes the playback of a paused song
        /// </summary>
        public void Resume()
        {
            PlaybackState = _nAudioWrapper.Play();
        }

        /// <summary>
        /// stops the current playback and unloads the player of the current song
        /// </summary>
        public void Stop()
        {
            PlaybackState = _nAudioWrapper.Stop();
        }

        /// <summary>
        /// event that fires when the player stops automatically
        /// </summary>
        public event EventHandler PlaybackEnded;

        /// <summary>
        /// calls the NAudioWrapper seek function
        /// </summary>
        /// <param name="timeToSeekTo"></param>
        public void Seek(TimeSpan timeToSeekTo)
        {
            _nAudioWrapper.Seek(timeToSeekTo);
        }

        /// <summary>
        /// gets the current time of the playing song
        /// </summary>
        public TimeSpan CurrentTime { get { return _nAudioWrapper.CurrentTime; } }

        /// <summary>
        /// gets the total time of the playing song
        /// </summary>
        public TimeSpan TotalTime { get { return _nAudioWrapper.TotalTime; } }
        #endregion

        #region private methods
        /// <summary>
        /// passes the event from _waveout to the caller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="stoppedEventArgs"></param>
        private void WaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (PlaybackEnded != null) PlaybackEnded(sender, stoppedEventArgs);
        }
        #endregion
    }
}
