using System;
using System.IO;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using TagLib;

namespace MSOE.MediaComplete.Lib
{
    public class Player
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

        /// <summary>
        /// sets up the player to play the file
        /// </summary>
        /// <param name="file">file to play</param>
        public void Play(FileInfo file)
        {
            if (file == null) return;//TODO: throw something

            if (_waveOut != null)
            {
                Stop();
            }
            try
            {
                switch (file.Extension.ToLower())
                {
                    case ".mp3":
                        _reader = new Mp3FileReader(file.FullName);
                        break;
                    case ".wav":
                        _reader = new WaveFileReader(file.FullName);
                        break;
                    case ".wma":
                        _reader = new WMAFileReader(file.FullName);
                        break;
                    default:
                        throw new UnsupportedFormatException(file.Extension + " is not supported");
                }
            }
            catch(Exception e)//TODO: Catch specifically
            {
                Stop();
                throw new CorruptFileException(file.FullName +
                                            " cannot be loaded, the file may be corrupt or have the wrong extension.", e);
            }

            _waveOut = new WaveOut();
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
        
        public void Seek()
        {
            throw new NotImplementedException("Seek is not yet implemented.");
            //_reader.Seek(10000000, SeekOrigin.Current);//seeks ahead 10000000 bytes in the file?
        }
    }
}
