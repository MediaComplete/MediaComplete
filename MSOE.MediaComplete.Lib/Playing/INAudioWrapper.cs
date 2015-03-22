using System;
using System.IO;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    public interface INAudioWrapper
    {
        void Setup(FileInfo fileInfo, EventHandler<StoppedEventArgs> handler, double currentVolume);
        PlaybackState Play();
        PlaybackState Pause();
        PlaybackState Stop();
        void ChangeVolume(double sliderVolume);
    }
}