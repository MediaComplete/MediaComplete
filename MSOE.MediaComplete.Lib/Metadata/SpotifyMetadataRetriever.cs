using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MSOE.MediaComplete.Lib.Util;
using Newtonsoft.Json.Linq;
using TagLib;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Retrieves additional metadata by using the Spotify web API.
    /// </summary>
    internal class SpotifyMetadataRetriever : IMetadataRetriever
    {
        /// <summary>
        /// Gets the metadata for a song, based on the passed in artist, album, and track title.
        /// </summary>
        /// <param name="data">A metadata object to populate. We assume the title, artist, and album have already been populated</param>
        public async Task GetMetadataAsync(Metadata data)
        {
            var query = HttpUtility.UrlEncode(String.Format("artist:{0} album:{1} track:{2}", data.AlbumArtists.FirstOrDefault(), data.Album, data.Title));
            var url = "https://api.spotify.com/v1/search?type=track&limit=1&q=" + query;

            var json = await RequestWithAuthAsync(url);

            var items = json.SelectToken("tracks.items");
            if (items == null) // Format change, auth error, etc. 
            {
                throw new IOException("Unexpected response from metadata request");
            }

            var firstTrack = items.First;
            if (firstTrack == null) // No results found, return empty object
            {
                return;
            }

            await ParseMetadataAsync(data, firstTrack); // Return parsed data
        }

        /// <summary>
        /// Fill out and return a metadata object based on a metadata JSON. This method performs 
        /// secondary requests as necessary to obtain more information
        /// </summary>
        /// <param name="data">The metadata object to populate</param>
        /// <param name="json">The json of the song track</param>
        private async Task ParseMetadataAsync(Metadata data, JToken json)
        {
            // HTTP GET the album art first - Spotify provides several sizes, largest first.
            var imgJson = json.SelectToken("album.images[0].url");
            if (imgJson != null)
            {
                var bytes = await HttpUtil.RequestBytesAsync(imgJson.ToString(), "GET",new Dictionary<HttpRequestHeader, string>
                {
                    { HttpRequestHeader.Authorization, "Bearer " + _accessToken }
                });
                data.AlbumArt = new Picture(new ByteVector(bytes));
            }

            // The year, genre, and album artists also need a followup request to Spotify to get more album details
            var albumHref = json.SelectToken("album.href");
            if (albumHref != null)
            {
                var albumJson = await RequestWithAuthAsync(albumHref.ToString());
                var genreJson = albumJson["genres"] as JArray;
                if (genreJson != null && genreJson.Any())
                {
                    data.Genre = genreJson.Select(g => g.ToObject<string>()).Aggregate((g1, g2) => g1 + "; " + g2);
                }

                var dateJson = albumJson["release_date"];
                if (dateJson != null)
                {
                    // First 4 chars are the year; month and day might follow
                    var yearStr = dateJson.ToObject<string>().Substring(0, 4);
                    data.Year = uint.Parse(yearStr);
                }

                var albumArtists = albumJson["artists"] as JArray;
                if (albumArtists != null)
                {
                    data.AlbumArtists = albumArtists.Select(j => j["name"].ToObject<string>());
                }
            }

            var trackNumberJson = json["track_number"];
            if (trackNumberJson != null)
            {
                data.TrackNumber = trackNumberJson.ToObject<uint>();
            }

            var popJson = json["popularity"];
            if (popJson != null)
            {
                // Spotify's popularity is 0-100, ID3 rating is 0-255
                data.Rating = popJson.ToObject<uint>() * 255 / 100; 
            }

            var artists = json["artists"] as JArray;
            if (artists != null)
            {
                data.SupportingArtists = artists.Select(j => j["name"].ToObject<string>());
            }
        }

        #region Singleton handling/initialization

        private static SpotifyMetadataRetriever _instance;

        private SpotifyMetadataRetriever()
        {
        }

        /// <summary>
        /// Asynchronously access the singleton instance. This is necessary because initial 
        /// construction needs to request an authentication token.
        /// </summary>
        /// <returns>An awaitable instance of this class</returns>
        public static async Task<SpotifyMetadataRetriever> GetInstanceAsync()
        {
            if (_instance == null)
            {
                _instance = new SpotifyMetadataRetriever();
                await _instance.RefreshAccessTokenAsync();
            }
            return _instance;
        }

        #endregion

        #region Connection Management

        private string _accessToken;

        /// <summary>
        /// Get a new access token from Spotify
        /// </summary>
        /// <returns>An awaitable so you know when a new token has been obtained</returns>
        private async Task RefreshAccessTokenAsync()
        {
            const string url = "https://accounts.spotify.com/api/token";
            const string postString = "grant_type=client_credentials";

            var response = await HttpUtil.RequestJsonAsync(url, "POST", new Dictionary<HttpRequestHeader, string>
            {
                // Encoded on https://www.base64encode.org/
                { HttpRequestHeader.Authorization, "Basic YWI5ZWU4ZmM2NmFlNGU5NmIxYTI0ODJmZjNkYWE3OWU6YzNjNGFiNTczN2JjNDA1OTgyZjk5NWUyY2RiMmRiYTU="},
                { HttpRequestHeader.ContentType, "application/x-www-form-urlencoded" }
            }, Encoding.UTF8.GetBytes(postString));
            
            _accessToken = response["access_token"].ToString();
        }

        /// <summary>
        /// Helper method to perform a request to the Spotify API and re-authenticate as necessary
        /// </summary>
        /// <param name="url">The URL to hit</param>
        /// <returns>The JSON response</returns>
        private async Task<JToken> RequestWithAuthAsync(string url)
        {
            var json = await HttpUtil.RequestJsonAsync(url, "GET", new Dictionary<HttpRequestHeader, string>
            {
                { HttpRequestHeader.Authorization, "Bearer " + _accessToken }
            });

            // Refresh our token if necessary
            var err = json.SelectToken("error.status");
            if (err != null && err.ToObject<int>() == 401)
            {
                await RefreshAccessTokenAsync();
                // Redo the request
                json = await HttpUtil.RequestJsonAsync(url, "GET", new Dictionary<HttpRequestHeader, string>
                {
                    { HttpRequestHeader.Authorization, "Bearer " + _accessToken }
                });
            }

            return json;
        }

        #endregion
    }
}