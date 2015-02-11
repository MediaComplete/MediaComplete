using System;
using System.IO;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using TagLib;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// concrete implementation of the wavestream factory
    /// </summary>
    public class NAudioWrapper : INAudioWrapper
    {

        /// <summary>
        /// returns the proper wavestream based on the given filetype
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public WaveStream GetWaveStream(FileInfo file)
        {
            try
            {
                switch (file.Extension.ToLower())
                {
                    case Constants.Mp3FileExtension:
                        return new Mp3FileReader(file.FullName);
                    case Constants.WavFileExtension:
                        return new WaveFileReader(file.FullName);
                    case Constants.WmaFileExtension:
                        return new WMAFileReader(file.FullName);
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
        }

        public WaveOut GetWaveOut(WaveStream waveStream, EventHandler<StoppedEventArgs> handler)
        {
            var waveOut = new WaveOut();
            waveOut.Init(waveStream);
            waveOut.PlaybackStopped += handler;
            return waveOut;
        }

        public PlaybackState Play(WaveOut waveOut)
        {
            if (waveOut == null) return PlaybackState.Stopped;
            waveOut.Play();
            return waveOut.PlaybackState;
        }

        public PlaybackState Pause(WaveOut waveOut)
        {
            if (waveOut == null) return PlaybackState.Stopped;
            waveOut.Pause();
            return waveOut.PlaybackState;
        }

        public PlaybackState Stop(ref WaveOut waveOut, ref WaveStream waveStream)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (waveStream != null)
            {
                waveStream.Dispose();
                waveStream = null;
            }
            return PlaybackState.Stopped;
        }
    }
}