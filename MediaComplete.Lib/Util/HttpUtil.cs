using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MediaComplete.Lib.Util
{
    static class HttpUtil
    {
        /// <summary>
        /// Performs a request to a JSON web API and returns the result as a JToken
        /// </summary>
        /// <param name="url">The url to hit, including prototcol, address, path, and params</param>
        /// <param name="verb">The HTTP verb to use (e.g. "GET" or "POST")</param>
        /// <param name="headers">Optional dictionary of additional HTTP headers to include</param>
        /// <param name="reqBody">Optional byte array to include the request body.</param>
        /// <returns>JToken result of the query</returns>
        public async static Task<JToken> RequestJsonAsync (string url, string verb,
            Dictionary<HttpRequestHeader, string> headers = null, byte[] reqBody = null)
        {
            var bytes = await RequestBytesAsync(url, verb, headers, reqBody);

            return JToken.Parse(Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// Returns a byte array from a webrequest.
        /// </summary>
        /// <param name="url">The url to hit, including prototcol, address, path, and params</param>
        /// <param name="verb">The HTTP verb to use (e.g. "GET" or "POST")</param>
        /// <param name="headers">Optional dictionary of additional HTTP headers to include</param>
        /// <param name="reqBody">Optional byte array to include the request body.</param>
        /// <returns>A byte array response from the server</returns>
        public async static Task<byte[]> RequestBytesAsync(string url, string verb,
            Dictionary<HttpRequestHeader, string> headers = null, byte[] reqBody = null)
        {
            // Build request
            var request = WebRequest.Create(url);
            request.Method = verb;
            if (headers != null)
            {
                // WebRequest protects some headers and makes us apply via property
                // http://stackoverflow.com/a/4752359/3642834
                string contentType;
                var hasKey = headers.TryGetValue(HttpRequestHeader.ContentType, out contentType);
                if (hasKey)
                {
                    headers.Remove(HttpRequestHeader.ContentType);
                    request.ContentType = contentType;
                }

                foreach (var entry in headers)
                {
                    request.Headers.Add(entry.Key, entry.Value);
                }
            }
            if (reqBody != null)
            {
                request.ContentLength = reqBody.Length;
                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(reqBody, 0, reqBody.Length);
                }
            }

            // Perform request
            byte[] ret = null;
            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    ret = new byte[response.ContentLength];

                    var i = 0;
                    do
                    {
                        i += responseStream.Read(ret, i, ret.Length - i);
                    } while (i < ret.Length);
                }
            }
            return ret;
        }
    }
}
