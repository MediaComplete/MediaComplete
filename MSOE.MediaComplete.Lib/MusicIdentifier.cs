using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ENMFPdotNet;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json.Linq;

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
            // We have to force "SampleAudio" onto a new thread, otherwise the main thread 
            // will lock while doing the expensive file reading and audio manipulation.
            if (!File.Exists(filename)) return null;
            var audioData = await Task.Run(() => SampleAudio(filename));
            var codegen = new FingerprintGenerator(audioData, 0);
            var code = codegen.GetFingerprintCode().Code;

            var client = new HttpClient {BaseAddress = new Uri(Url)};
            // TODO lookup and add any metadata fields already on the file
            var response = await client.GetAsync(Path + "?api_key=" + ApiKey + "&code=" + code);

            // Parse the response body.
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            // TODO - return useful data and handle failed identification...
            var title = json.SelectToken("response.songs[0].title").ToString();
            return title;
        }

        /*
         * Parses an MP3 file and pulls the first 30 seconds into the format needed for the Echonest code generator
         */

        private static float[] SampleAudio(string filename)
        {
            string inFile = filename;
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