using System;
using System.IO;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;

namespace MSOE.MediaComplete.Lib
{
    public class Player
    {
        private WaveOut _waveOut;
        private WaveStream _reader;
        private static Player _instance;

        public static Player Instance
        {
            get
            {
                return _instance ?? (_instance = new Player());
            }
        }

        private Player() { }

        public void Play(FileInfo file)
        {
            if (file == null) return;
            
            if (_waveOut != null)
            {
                Stop();
            }

            switch (file.Extension.ToUpper())
            {
                case "MP3":
                    _reader = new Mp3FileReader(file.FullName);
                    break;
                case "WAV":
                    _reader = new WaveFileReader(file.FullName);
                    break;
                case "WMA":
                    _reader = new WMAFileReader(file.FullName);
                    break;
                default:
                    return;
            }

            _waveOut = new WaveOut();
            _waveOut.Init(_reader);
            _waveOut.Play();
        }

        public void Pause()
        {
            if (_waveOut == null) return;
            _waveOut.Pause();
        }

        public void Resume()
        {
            if (_waveOut == null) return;
            _waveOut.Resume();
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
        }

        public void Seek()
        {
            throw new NotImplementedException("Seek is not yet implemented.");
            //_reader.Seek(10000000, SeekOrigin.Current);//seeks ahead 10000000 bytes in the file?
        }

        public PlaybackState PlaybackState()
        {
            return _waveOut.PlaybackState;
        }
    }
}
