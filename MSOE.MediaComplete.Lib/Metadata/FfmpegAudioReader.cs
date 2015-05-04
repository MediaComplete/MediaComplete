using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Files;

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
        /// <param name="file">The target file</param>
        /// <param name="frequency">The expected frequency of the sampled data</param>
        /// <param name="sampleSeconds">The duration of the audio sample</param>
        /// <returns>An array of sampled byte data</returns>
        public async Task<byte[]> ReadBytesAsync(LocalSong file, int frequency, uint sampleSeconds)
        {
            var wavData = new byte[frequency * sampleSeconds * 2];

            await Task.Run(delegate
            {
                var ffmpeg = new Process
                {
                    StartInfo =
                    {
                        FileName = "ffmpeg\\ffmpeg.exe",
                        Arguments = String.Format("-i \"{0}\" -ac 1 -ar {1} -f wav -", file.GetPath(), frequency),
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

            return wavData;
        }
    }
}
