using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Parse audio data by spawning an Fffmpeg process.
    /// </summary>
    internal class FfmpegAudioReader : IAudioReader
    {
        /// <summary>
        /// Reads audio bytes in from ffmpeg
        /// </summary>
        /// <param name="filename">The target file path</param>
        /// <param name="frequency">The expected frequency of the sampled data</param>
        /// <param name="sampleSeconds">The duration of the audio sample</param>
        /// <returns>An array of sampled byte data</returns>
        public async Task<byte[]> ReadBytesAsync(string filename, int frequency, uint sampleSeconds)
        {
            var wavData = new byte[frequency * sampleSeconds * 2];
            var newFormat = new WaveFormat(frequency, 16, 1); // 16-bit quality, mono-channel

            await Task.Run(delegate
            {
                var ffmpeg = new Process
                {
                    StartInfo =
                    {
                        FileName = "ffmpeg\\ffmpeg.exe",
                        Arguments = String.Format("-i \"{0}\" -ac 1 -ar {1} -f wav -", filename, frequency),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                ffmpeg.Start();
                var reader = new BinaryReader(ffmpeg.StandardOutput.BaseStream);
                var count = 0;
                while (count < wavData.Length - 1)
                {
                    count += reader.Read(wavData, count, wavData.Length - count);
                }

                ffmpeg.Kill();
                ffmpeg.WaitForExit(2000);
                // This can return false if FFMPEG won't cooperate, but there's not much we can do about that.
            });

#if DEBUG
            // Write back to file, so we can make sure it's a good (albeit lower-quality) wav
            using (var writer = new WaveFileWriter(filename + "-sampled.wav", newFormat))
            {
                writer.Write(wavData, 0, wavData.Length);
                writer.Flush();
            }
#endif
            return wavData;
        }
    }
}
