using ENMFPdotNet;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
    public class MusicIdentifier
    {
        private static readonly int FREQ = 22050;
        private static readonly int SAMPLE_SECONDS = 30;
        private static readonly int SAMPLE_SIZE = FREQ * SAMPLE_SECONDS;
        private static readonly string URL = "http://developer.echonest.com";
        private static readonly string PATH = "/api/v4/song/identify";
        private static readonly string API_KEY = "MUIGA58IV1VQUOEJ5";

        public static async Task<string> IdentifySong(string filename) {
            // We have to force "SampleAudio" onto a new thread, otherwise the main thread 
            // will lock while doing the expensive file reading and audio manipulation.
            float[] audioData = await Task.Run(() => SampleAudio(filename));
            var codegen = new FingerprintGenerator(audioData, 0);
            var code = codegen.GetFingerprintCode().Code;

            var client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            // TODO lookup and add any metadata fields already on the file
            var response = await client.GetAsync(PATH + "?api_key=" + API_KEY + "&code=" + code); 

            // Parse the response body.
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            // TODO - return useful data
            var title = json.SelectToken("response.songs[0].title").ToString();
            return title;
        }

        /*
         * Parses an MP3 file and pulls the first 30 seconds into the format needed for the Echonest code generator
         */
        private static float[] SampleAudio(string filename)
        {
            var inFile = filename;
            List<float> result = new List<float>();

            using (var reader = new Mp3FileReader(inFile))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    var format = new WaveFormat(FREQ, 1);
                    using (WaveStream readerStream = new WaveFormatConversionStream(format, pcmStream))
                    {
                        ISampleProvider provider = new Pcm16BitToSampleProvider(readerStream); // Assumes 16 bit... problem?
                        // Read blocks of samples until no more available
                        int blockSize = 2000;
                        float[] buffer = new float[blockSize];
                        int rc;
                        while ((result.Count + blockSize) < SAMPLE_SIZE && (rc = provider.Read(buffer, 0, blockSize)) > 0)
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
