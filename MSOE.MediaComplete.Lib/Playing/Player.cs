using System;
using MSOE.MediaComplete.Lib.Files;
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
            get { return _instance ?? (_instance = new Player(new NAudioWrapper(), Library.Instance)); }
        }

        /// <summary>
        /// private constructor to prevent creation of more than one Player instance
        /// </summary>
        internal Player(INAudioWrapper nAudioWrapper, ILibrary library)
        {
            _nAudioWrapper = nAudioWrapper;
            _library = library;
        }
        #endregion

        #region properties
        /// <summary>
        /// factory used to get the WaveStream
        /// </summary>
        private readonly INAudioWrapper _nAudioWrapper;

        private readonly ILibrary _library;
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
            //TODO refactor so Player doesnt use a reference to NowPlaying MC-23
            var song = NowPlaying.Inst.CurrentSong();

            var localSong = song as LocalSong;
            if (localSong != null)
            {
                _nAudioWrapper.Setup(localSong, WaveOutOnPlaybackStopped, _currentVolume);
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

        #endregion

        #region private methods
        //TODO refactor so player doesnt use a reference to NowPlaying MC-23
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
                PlaylistFinishedEvent();
            }
        }
        
        /// <summary>
        /// event for end of song
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public delegate void SongFinished(int oldPath, int newPath);
        public event SongFinished SongFinishedEvent = delegate { };

        /// <summary>
        /// event for end of playlist
        /// TODO fix this and the one from NowPlaying MC-23
        /// </summary>
        public delegate void PlaylistFinished();
        public event PlaylistFinished PlaylistFinishedEvent = delegate { };
        #endregion

        protected void OnSongFinishedEvent(int oldIndex , int newIndex )
        {
            SongFinishedEvent(oldIndex, newIndex);
        }
    }
}
