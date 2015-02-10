using System;
using System.IO;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using TagLib;

namespace MSOE.MediaComplete.Lib
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
            get { return _instance ?? (_instance = new Player()); }
        }

        /// <summary>
        /// private constructor to prevent creation of more than one Player instance
        /// </summary>
        private Player() { }
        #endregion

        #region properties
        /// <summary>
        /// the wave object that controls the play/pause of the song
        /// </summary>
        private WaveOut _waveOut;

        /// <summary>
        /// the reader used by the _waveOut to get bytes from the files
        /// </summary>
        private WaveStream _reader;

        /// <summary>
        /// the state of the player
        /// </summary>
        public PlaybackState PlaybackState { get; private set; }
        #endregion

        #region IPlayer methods
        /// <summary>
        /// sets up the player to play the file
        /// </summary>
        /// <param name="file">file to play</param>
        public void Play(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");

            Stop();

            try
            {
                switch (file.Extension.ToLower())
                {
                    case Constants.Mp3FileExtension:
                        _reader = new Mp3FileReader(file.FullName);
                        break;
                    case Constants.WavFileExtension:
                        _reader = new WaveFileReader(file.FullName);
                        break;
                    case Constants.WmaFileExtension:
                        _reader = new WMAFileReader(file.FullName);
                        break;
                    default:
                        throw new UnsupportedFormatException(file.Extension + " is not supported");
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException || ex is IndexOutOfRangeException || ex is UnsupportedFormatException || ex is CorruptFileException)
                {
                    throw new CorruptFileException(file.FullName +
                        " cannot be loaded, the file may be corrupt or have the wrong extension.", ex);
                }
                throw;
            }

            _waveOut = new WaveOut();
            _waveOut.PlaybackStopped += WaveOutOnPlaybackStopped;
            _waveOut.Init(_reader);
            _waveOut.Play();
            PlaybackState = PlaybackState.Playing;
        }

        /// <summary>
        /// pauses the current playback
        /// </summary>
        public void Pause()
        {
            if (_waveOut == null) return;
            _waveOut.Pause();
            PlaybackState = PlaybackState.Paused;
        }

        /// <summary>
        /// resumes the playback of a paused song
        /// </summary>
        public void Resume()
        {
            if (_waveOut == null) return;
            _waveOut.Play();
            PlaybackState = PlaybackState.Playing;
        }

        /// <summary>
        /// stops the current playback and unloads the player of the current song
        /// </summary>
        public void Stop()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
            PlaybackState = PlaybackState.Stopped;
        }

        /// <summary>
        /// event that fires when the player stops automatically
        /// </summary>
        public event EventHandler PlaybackEnded;

        //public void Seek()
        //{
        //    //TODO: MC-41 - Seeking functionality
        //    throw new NotImplementedException("Seek is not yet implemented.");
        //    //_reader.Seek(10000000, SeekOrigin.Current);//seeks ahead 10000000 bytes in the file?
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
