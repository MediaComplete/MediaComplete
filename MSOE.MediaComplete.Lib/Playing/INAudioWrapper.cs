using System;
using System.IO;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Playing
{
    public interface INAudioWrapper
    {
        WaveStream GetWaveStream(FileInfo file);
        WaveOut GetWaveOut(WaveStream waveStream, EventHandler<StoppedEventArgs> handler);
        PlaybackState Play(WaveOut waveOut);
        PlaybackState Pause(WaveOut waveOut);
        PlaybackState Stop(ref WaveOut waveOut, ref WaveStream waveStream);
    }
}