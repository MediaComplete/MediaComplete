using System;
using System.IO;
using MSOE.MediaComplete.Lib.Files;
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
        /// returns the proper wavestream based on the given file-type
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>An audio wavestream</returns>
        /// <exception cref="System.ArgumentNullException">If the file is null</exception>
        /// <exception cref="TagLib.CorruptFileException">If the specified file is corrupted</exception>
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
        /// <param name="localSong">The song file</param>
        /// <param name="handler">The callback for when the song events</param>
        /// <param name="currentVolume">The current volume</param>
        public void Setup(LocalSong localSong, EventHandler<StoppedEventArgs> handler, double currentVolume)
        {
            _waveOut = new WaveOut();
            var waveStream = GetWaveStream(new FileInfo(localSong.Path));
            ChangeVolume(currentVolume);
            _waveOut.Init(_waveStream = waveStream);
            _waveOut.PlaybackStopped += handler;
        }

        /// <summary>
        /// Plays the file
        /// </summary>
        /// <returns>
        /// A state confirmation
        /// </returns>
        public PlaybackState Play()
        {
            if (_waveOut == null) return PlaybackState.Stopped;
            _waveOut.Play();
            return _waveOut.PlaybackState;
        }

        /// <summary>
        /// Pauses the file
        /// </summary>
        /// <returns>
        /// A state confirmation
        /// </returns>
        public PlaybackState Pause()
        {
            if (_waveOut == null) return PlaybackState.Stopped;
            _waveOut.Pause();
            return _waveOut.PlaybackState;
        }

        /// <summary>
        /// stops playback and requires the setup to be called on the next song
        /// </summary>
        /// <returns>
        /// A state confirmation
        /// </returns>
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
        /// <param name="timeToSeekTo">The time to seek to.</param>
        public void Seek(TimeSpan timeToSeekTo)
        {
            if (_waveOut == null || _waveStream == null) return;
            if (_waveStream.CanSeek)
            {
                _waveStream.CurrentTime = timeToSeekTo;
            }
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

        /// <summary>
        /// Changes the volume.
        /// </summary>
        /// <param name="sliderVolume">The slider volume.</param>
        public void ChangeVolume(double sliderVolume)
        {
            if (_waveOut != null)
                _waveOut.Volume = (float)(sliderVolume/200.0);
        }
    }
}