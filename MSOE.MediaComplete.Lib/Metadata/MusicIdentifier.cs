using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using File = TagLib.File;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Provides functions for identifying a song
    /// </summary>
    public static class MusicIdentifier
    {
        private const int Freq = 8000;
        private const int SampleSeconds = 10;

        /// <summary>
        /// Identify a song; restoring its metadata based on the audio data
        /// </summary>
        /// <param name="fileMover">Service for accessing the song</param>
        /// <param name="filename">The name of the target song</param>
        /// <returns></returns>
        public static async Task IdentifySongAsync(FileMover fileMover, string filename)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Started", StatusBarHandler.StatusIcon.Working);

            if (!fileMover.FileExists(filename))
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException", StatusBarHandler.StatusIcon.Error);
                return;
            }
            
            var audioData = await SampleAudioAsync(filename);
            if (audioData == null)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException", StatusBarHandler.StatusIcon.Error);
                return;
            }

            var client = new HttpClient { BaseAddress = new Uri(@"http://developer.doreso.com") };

            var payload = new ByteArrayContent(audioData);
            payload.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync(@"/api/v1/song/identify?api_key=IZY34FSerDqD8NiP2mDBTIqG4gOSeHuMHQqsZaekvRM", payload);

            // Parse the response body.
            var strResponse = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(strResponse);

            UpdateFileWithJson(json, fileMover.CreateTaglibFile(filename));

            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Success", StatusBarHandler.StatusIcon.Success);
        }

        /// <summary>
        /// Replace the metadata attributes with the new data obtained from the web
        /// </summary>
        /// <param name="json">The JSON web data. Currently assumed to be in Doreso's format</param>
        /// <param name="metadata">The metadata object to repopulate</param>
        private static void UpdateFileWithJson(JToken json, File metadata)
        {
            const string song = "data[0].";
            var title = json.SelectToken(song + "name");
            if (title != null)
            {
                metadata.SetAttribute(MetaAttribute.SongTitle, title.ToString());
            }
            var artist = json.SelectToken(song + "artist_name");
            if (artist != null)
            {
                metadata.SetAttribute(MetaAttribute.Artist, artist.ToString());
            }
            var album = json.SelectToken(song + "album");
            if (album != null)
            {
                metadata.SetAttribute(MetaAttribute.Album, album.ToString());
            }

            // TODO (MC-139, MC-45) add more - this will require using the ID passed in to access possible other databases...further research needed
        }

        /// <summary>
        /// Read in audio data to send to the web identification service
        /// </summary>
        /// <param name="filename">The filename to read from</param>
        /// <returns>A byte array of wav data</returns>
        private async static Task<byte[]> SampleAudioAsync(string filename)
        {
            var wavData = new byte[Freq * SampleSeconds * 2];
            var newFormat = new WaveFormat(Freq, 16, 1); // 16-bit quality, mono-channel

            await Task.Run(delegate
            {
                var ffmpeg = new Process
                {
                    StartInfo =
                    {
                        FileName = "ffmpeg\\ffmpeg.exe",
                        Arguments = String.Format("-i \"{0}\" -ac 1 -ar {1} -f wav -", filename, Freq),
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

                if (ffmpeg.WaitForExit(2000))
                {
                }
                else
                {
                    // TODO this is bad This Is Bad THIS IS BAD
                }
            });

#if DEBUG
            // Write back to file, make sure it's a good (albeit low-quality) wav
            using (var writer = new WaveFileWriter(filename + "-sampled.wav", newFormat))
            {
                writer.Write(wavData, 0, wavData.Length);
                writer.Flush();
            }
#endif
            return wavData;
        }
    }

    /// <summary>
    /// Represents an error in the identification process
    /// </summary>
    public class IdentificationException : Exception
    {
        public IdentificationException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}