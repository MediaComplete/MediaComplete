using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ENMFPdotNet;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json.Linq;
using TagLib;

namespace MSOE.MediaComplete.Lib.Metadata
{
    public static class MusicIdentifier
    {
        private const int Freq = 22050;
        private const int SampleSeconds = 30;
        private const int SampleSize = Freq*SampleSeconds;
        private static readonly UriBuilder Uri = new UriBuilder("http", "developer.echonest.com");
        private const string Path = "/api/v4/song/identify";
        private const string ApiKey = "MUIGA58IV1VQUOEJ5";

        public static async Task<string> IdentifySongAsync(FileManager fileMover, string filename)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Started", StatusBarHandler.StatusIcon.Working);

            if (!fileMover.FileExists(filename))
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException", StatusBarHandler.StatusIcon.Error);
                return null;
            }
            // We have to force "SampleAudio" onto a new thread, otherwise the main thread 
            // will lock while doing the expensive file reading and audio manipulation.
            var audioData = await Task.Run(() => SampleAudio(fileMover, filename));
            if (audioData == null)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException", StatusBarHandler.StatusIcon.Error);
                return null;
            }

            var codegen = new FingerprintGenerator(audioData, 0);
            var code = codegen.GetFingerprintCode().Code;

            var client = new HttpClient {BaseAddress = new Uri(Uri.ToString())};

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["api_key"] = ApiKey;
            query["code"] = code;
            var response = await client.GetAsync(Path + "?" + query);

            // Parse the response body.
            var strResponse = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(strResponse);

            UpdateFileWithJson(json, fileMover.CreateTaglibFile(filename));

            var resp = json.SelectToken("response").ToString();
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Success", StatusBarHandler.StatusIcon.Success);
            return resp;
        }

        private static void UpdateFileWithJson(JToken json, File create)
        {
            const string song = "response.songs[0].";
            var title = json.SelectToken(song + "title");
            if (title!=null)
            {
                create.SetAttribute(MetaAttribute.SongTitle, title.ToString());
            }
            var artist = json.SelectToken(song + "artist_name");
            if (artist != null)
            {
                create.SetAttribute(MetaAttribute.Artist, artist.ToString());
            }

            // TODO (MC-139, MC-45) add more - this will require using the ID passed in to access possible other databases...further research needed
        }

        /*
         * Parses an MP3 file and pulls the first 30 seconds into the format needed for the Echonest code generator
         */
        private static float[] SampleAudio(IFileManager fileManager, string filename)
        {
            var inFile = filename;

            if (!fileManager.FileExists(inFile)) return null;

            var result = new List<float>();

            try
            {
                using (var reader = new Mp3FileReader(inFile))
                {
                    using (var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        var format = new WaveFormat(Freq, 1);
                        using (var readerStream = new WaveFormatConversionStream(format, pcmStream))
                        {
                            var provider = new Pcm16BitToSampleProvider(readerStream);
                            // Assumes 16 bit... works with all the fields we've tested, but we might need to enhance this later.
                            // Read blocks of samples until no more available
                            const int blockSize = 2000;
                            var buffer = new float[blockSize];
                            int rc;
                            while ((result.Count + blockSize) < SampleSize &&
                                   (rc = provider.Read(buffer, 0, blockSize)) > 0)
                            {
                                result.AddRange(buffer.Take(rc));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new IdentificationException(String.Format(
                    "Could not identify music file {0}. It could be in use by another program, or not in a format we recognize.",
                    filename), e);
            }
            return result.ToArray();
        }
    }

    public class IdentificationException : Exception
    {
        public IdentificationException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}