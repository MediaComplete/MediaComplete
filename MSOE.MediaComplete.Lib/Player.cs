using System;
using System.IO;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;

namespace MSOE.MediaComplete.Lib
{
    public class Player
    {
        #region singleton stuff
        private static Player _instance;

        public static Player Instance
        {
            get { return _instance ?? (_instance = new Player()); }
        }

        private Player()
        {
        }
        #endregion

        private WaveOut _waveOut;
        private WaveStream _reader;
        public PlaybackState PlaybackState { get; private set; }

        public void Play(FileInfo file)
        {
            if (file == null) return;

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
                        return;
                }
            }
            catch
            {
                throw new FileLoadException(file.FullName +
                                            " cannot be loaded, the file may be corrupt or have the wrong extension.");
            }

            _waveOut = new WaveOut();
            _waveOut.Init(_reader);
            _waveOut.Play();
            PlaybackState = PlaybackState.Playing;
        }

        public void Pause()
        {
            if (_waveOut == null) return;
            _waveOut.Pause();
            PlaybackState = PlaybackState.Paused;
        }

        public void Resume()
        {
            if (_waveOut == null) return;
            _waveOut.Play();
            PlaybackState = PlaybackState.Playing;
        }

        public void Stop()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _reader.Dispose();
                _waveOut = null;
            }
            _reader = null;
            PlaybackState = PlaybackState.Stopped;
        }

        public void Seek()
        {
            throw new NotImplementedException("Seek is not yet implemented.");
            //_reader.Seek(10000000, SeekOrigin.Current);//seeks ahead 10000000 bytes in the file?
        }
    }
}
