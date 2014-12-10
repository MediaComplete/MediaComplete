using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ENMFPdotNet;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json.Linq;
using File = TagLib.File;

namespace MSOE.MediaComplete.Lib
{
    public class MusicIdentifier
    {
        private const int Freq = 22050;
        private const int SampleSeconds = 30;
        private const int SampleSize = Freq*SampleSeconds;
        private const string Url = "http://developer.echonest.com";
        private const string Path = "/api/v4/song/identify";
        private const string ApiKey = "MUIGA58IV1VQUOEJ5";

        public static async Task<string> IdentifySong(string filename)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Started", StatusBarHandler.StatusIcon.Working);
            // We have to force "SampleAudio" onto a new thread, otherwise the main thread 
            // will lock while doing the expensive file reading and audio manipulation.
            if (!System.IO.File.Exists(filename))
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error", StatusBarHandler.StatusIcon.Error);
                return null;
            }
            var audioData = await Task.Run(() => SampleAudio(filename));
            if (audioData == null)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error", StatusBarHandler.StatusIcon.Error);
                return null;
            }
            var codegen = new FingerprintGenerator(audioData, 0);
            var code = codegen.GetFingerprintCode().Code;

            var client = new HttpClient {BaseAddress = new Uri(Url)};
            // TODO lookup and add any metadata fields already on the file
            var response = await client.GetAsync(Path + "?api_key=" + ApiKey + "&code=" + code);

            // Parse the response body.
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            UpdateFileWithJson(json, File.Create(filename));

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
                create.SetMetaAttribute(MetaAttribute.SongTitle,title.ToString());
            }
            var artist = json.SelectToken(song + "artist_name");
            if (artist != null)
            {
                create.SetMetaAttribute(MetaAttribute.Artist,artist.ToString());
            }
            //TODO add more - this will require using the ID passed in to access possible other databases...further research needed
        }

        /*
         * Parses an MP3 file and pulls the first 30 seconds into the format needed for the Echonest code generator
         */

        private static float[] SampleAudio(string filename)
        {
            var inFile = filename;

            if (!System.IO.File.Exists(inFile)) return null;

            var result = new List<float>();

            using (var reader = new Mp3FileReader(inFile)) //TODO BJK -- handle null files/other exceptions
            {
                using (var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    var format = new WaveFormat(Freq, 1);
                    using (var readerStream = new WaveFormatConversionStream(format, pcmStream))
                    {
                        var provider = new Pcm16BitToSampleProvider(readerStream);
                            // Assumes 16 bit... TODO is this a problem?
                        // Read blocks of samples until no more available
                        const int blockSize = 2000;
                        var buffer = new float[blockSize];
                        int rc;
                        while ((result.Count + blockSize) < SampleSize && (rc = provider.Read(buffer, 0, blockSize)) > 0)
                        {
                            result.AddRange(buffer.Take(rc));
                        }
                    }
                }
            }

            return result.ToArray();
        }
    }
}