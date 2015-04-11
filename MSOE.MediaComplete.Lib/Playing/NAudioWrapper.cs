using System;
using System.IO;
using NAudio.Wave;
using TagLib;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// concrete implementation of the wavestream factory
    /// </summary>
    public class NAudioWrapper : INAudioWrapper
    {
        private WaveStream _waveStream;
        private WaveOut _waveOut;

        /// <summary>
        /// returns the proper wavestream based on the given filetype
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static WaveStream GetWaveStream(FileSystemInfo file)
        {
            if(file == null) throw new ArgumentNullException("file");
            try
            {
                return new AudioFileReader(file.FullName) { Volume = 2.0f };
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
        }

        /// <summary>
        /// sets up the wrapper to play the given file
        /// required before each song to get it to begin playing
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="handler"></param>
        public void Setup(FileInfo fileInfo, EventHandler<StoppedEventArgs> handler, double currentVolume)
        {
            _waveOut = new WaveOut();
            var waveStream = GetWaveStream(fileInfo);
            ChangeVolume(currentVolume);
            _waveOut.Init(_waveStream = waveStream);
            _waveOut.PlaybackStopped += handler;
        }

        /// <summary>
        /// Plays the file
        /// </summary>
        /// <returns></returns>
        public PlaybackState Play()
        {
            if (_waveOut == null) return PlaybackState.Stopped;
            _waveOut.Play();
            return _waveOut.PlaybackState;
        }

        /// <summary>
        /// Pauses the file
        /// </summary>
        /// <returns></returns>
        public PlaybackState Pause()
        {
            if (_waveOut == null) return PlaybackState.Stopped;
            _waveOut.Pause();
            return _waveOut.PlaybackState;
        }

        /// <summary>
        /// stops playback and requires the setup to be called on the next song
        /// </summary>
        /// <returns></returns>
        public PlaybackState Stop()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            if (_waveStream != null)
            {
                _waveStream.Dispose();
                _waveStream = null;
            }
            return PlaybackState.Stopped;
        }

        /// <summary>
        /// seeks to the given time within the song (from the beginning)
        /// </summary>
        /// <param name="timeToSeekTo"></param>
        /// <returns></returns>
        public PlaybackState Seek(TimeSpan timeToSeekTo)
        {
            if (_waveOut == null || _waveStream == null) return PlaybackState.Stopped;
            if (_waveStream.CanSeek)
            {
                _waveStream.CurrentTime = timeToSeekTo;
            }

            return _waveOut.PlaybackState;
        }

        /// <summary>
        /// gets the total runtime of the current song or zero if one is not ready to be played
        /// </summary>
        public TimeSpan TotalTime
        {
            get { return _waveStream != null ? _waveStream.TotalTime : TimeSpan.FromMilliseconds(0); }
        }

        /// <summary>
        /// gets the current time of the current song or zero if one is not ready to be played
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return _waveStream != null ? _waveStream.CurrentTime : TimeSpan.FromMilliseconds(0); }
        }
        
        public void ChangeVolume(double sliderVolume)
        {
            if (_waveOut != null)
                _waveOut.Volume = (float)(sliderVolume/200.0);
        }
    }
}