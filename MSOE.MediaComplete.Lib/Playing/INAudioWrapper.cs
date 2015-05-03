using System;
using MSOE.MediaComplete.Lib.Files;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    public interface INAudioWrapper
    {
        void Setup(LocalSong localSong, EventHandler<StoppedEventArgs> handler, double currentVolume);
        PlaybackState Play();
        PlaybackState Pause();
        PlaybackState Stop();
        void Seek(TimeSpan timeToSeekTo);
        void ChangeVolume(double sliderVolume);
        TimeSpan TotalTime { get; }
        TimeSpan CurrentTime { get; }
    }
}