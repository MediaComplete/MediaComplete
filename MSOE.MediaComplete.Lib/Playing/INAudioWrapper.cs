using System;
using System.IO;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    public interface INAudioWrapper
    {
        void Setup(FileInfo fileInfo, EventHandler<StoppedEventArgs> handler);
        PlaybackState Play();
        PlaybackState Pause();
        PlaybackState Stop();
        PlaybackState Seek(TimeSpan timeToSeekTo);
        TimeSpan TotalTime { get; }
        TimeSpan CurrentTime { get; }
    }
}