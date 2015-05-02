using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Util;
using Newtonsoft.Json.Linq;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Uses Doreso to identify the song. Doreso returns the song title, album, and artist.
    /// </summary>
    internal class DoresoIdentifier : IAudioIdentifier
    {
        /// <summary>
        /// Perform a web request to identify the given audio data.
        /// </summary>
        /// <param name="audioBytes">The song data to analyze and identify</param>
        /// <returns>A partially constructed metadata object with the new data</returns>
        public async Task<Metadata> IdentifyAsync(byte[] audioBytes)
        {
            // Fire off the request
            const string url =
                "http://developer.doreso.com/api/v1/song/identify?api_key=IZY34FSerDqD8NiP2mDBTIqG4gOSeHuMHQqsZaekvRM";
            var json = await HttpUtil.RequestJsonAsync(url, "POST", new Dictionary<HttpRequestHeader, string>
            {
                {HttpRequestHeader.ContentType, "application/octet-stream"}
            }, audioBytes);
            var data = json["data"] as JArray;
            var result = json["status"].ToObject<uint>();

            if (data == null && result != 0) // API busy, auth error, etc.
            {
                throw new IdentificationException(String.Format("Received unexpected response from Doreso: {0}", json));
            }

            Metadata metadata = new Metadata();

            if (data == null) // No matches
            {
                return metadata;
            }

            var songJson = data.First;

            var titleToken = songJson.SelectToken("name");
            if (titleToken != null)
            {
                metadata.Title = titleToken.ToString();
            }
            var artistToken = songJson.SelectToken("artist_name");
            if (artistToken != null)
            {
                metadata.AlbumArtists = new List<string> { artistToken.ToString() };
            }
            var albumToken = songJson.SelectToken("album");
            if (albumToken != null)
            {
                metadata.Album = albumToken.ToString();
            }

            return metadata;
        }
    }
}
