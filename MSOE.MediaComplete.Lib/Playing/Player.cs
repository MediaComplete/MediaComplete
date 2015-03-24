using System;
using MSOE.MediaComplete.Lib.Songs;
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
        /// sets up the player and plays the song
        /// </summary>
        /// <param name="song">song to play</param>
        public void Play(AbstractSong song)
        {
            if (song == null) throw new ArgumentNullException("song");

            Stop();

            var localSong = song as LocalSong;
            if (localSong != null)
            {
                _nAudioWrapper.Setup(localSong.File, WaveOutOnPlaybackStopped);
            }
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

        //public void Seek()
        //{
        //    //TODO: MC-41 - Seeking functionality
        //    throw new NotImplementedException("Seek is not yet implemented.");
        //    //_waveStream.Seek(10000000, SeekOrigin.Current);//seeks ahead 10000000 bytes in the file?
        //}
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
