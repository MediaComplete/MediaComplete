using System;
using MSOE.MediaComplete.Lib.Songs;
using NAudio.Wave;
using TagLib;

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
        /// the current volume of the player
        /// </summary>
        private static double _currentVolume = 1.0f;

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
        public void Play()
        {
            Stop();
            var song = NowPlaying.Inst.CurrentSong();

            var localSong = song as LocalSong;
            if (localSong != null)
            {
                _nAudioWrapper.Setup(localSong.File, WaveOutOnPlaybackStopped, _currentVolume);
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

        /// <summary> 
        /// changes the volume of a currently playing song
        /// </summary>
        /// <param name="newValue"></param>
        public void ChangeVolume(double newValue)
        {
            if (PlaybackState != PlaybackState.Stopped)
            {
                _nAudioWrapper.ChangeVolume(newValue);
                _currentVolume = newValue;
            }
            else
                _currentVolume = newValue;
        }

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
            var oldIndex = NowPlaying.Inst.Index;
            if (NowPlaying.Inst.HasNextSong())
            {
                var canPlay = true;
                while (canPlay) { 
                    NowPlaying.Inst.NextSong();
                    try
                    {
                        Play();
                        canPlay = false;
                    }
                    catch (CorruptFileException)
                    {
                        if (!NowPlaying.Inst.HasNextSong())
                            canPlay = false;
                    }
                }
                var newIndex = NowPlaying.Inst.Index;
                OnSongFinishedEvent(oldIndex, newIndex);
            }
            else
            {
                if (PlaybackEnded != null) PlaybackEnded(sender, stoppedEventArgs);
                PlaybackState = PlaybackState.Stopped;
                OnSongFinishedEvent(-1,-1);
            }
        }
        public delegate void SongFinished(int oldPath, int newPath);

        public event  SongFinished SongFinishedEvent = delegate { };
        #endregion

        protected void OnSongFinishedEvent(int oldIndex , int newIndex )
        {
            SongFinishedEvent(oldIndex, newIndex);
        }
    }
}
