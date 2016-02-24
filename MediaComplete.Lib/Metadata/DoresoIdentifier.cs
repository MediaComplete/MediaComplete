using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MediaComplete.Lib.Library.DataSource;
using MediaComplete.Lib.Util;
using Newtonsoft.Json.Linq;

namespace MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Uses Doreso to identify the song. Doreso returns the song title, album, and artist.
    /// </summary>
    public class DoresoIdentifier : IAudioIdentifier
    {
        /// <summary>
        /// Perform a web request to identify the given audio data.
        /// </summary>
        /// <param name="audioBytes">The song data to analyze and identify</param>
        /// <param name="file">The file to begin populating with metadata</param>
        /// <returns>A partially constructed metadata object with the new data</returns>
        public async Task IdentifyAsync(byte[] audioBytes, LocalSong file)
        {
            // Fire off the request
            const string url =
                "http://developer.doreso.com/api/v1/song/identify?api_key=IZY34FSerDqD8NiP2mDBTIqG4gOSeHuMHQqsZaekvRM";
            var json = await HttpUtil.RequestJsonAsync(url, "POST", new Dictionary<HttpRequestHeader, string>
            {
                {HttpRequestHeader.ContentType, "application/octet-stream"}
            }, audioBytes);
            var data = json["data"] as JArray;
            var msg = json["msg"].ToObject<string>();

            // Doreso API key has been maxed out for now - show a warning and cancel the rest of the identifies
            if (data == null && msg.Contains("API key")) 
                throw new IdentificationException(String.Format("Received unexpected response from Doreso: {0}", json));
            
            // Doreso could not find a match
            if (data == null) 
                return;

            var songJson = data.First;

            var titleToken = songJson.SelectToken("name");
            if (titleToken != null)
                file.Title = titleToken.ToString();
            
            var artistToken = songJson.SelectToken("artist_name");
            if (artistToken != null)
                file.Artists = new List<string> { artistToken.ToString() };
            
            var albumToken = songJson.SelectToken("album");
            if (albumToken != null)
                file.Album = albumToken.ToString();
        }
    }
}
