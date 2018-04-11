using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApixuWeatherApi.Helpers
{
    internal class Http
    {
        internal static async Task<string> GetJson(Uri uri, bool compression = false)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (compression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ApixuWeatherApiClient", "1.0.0.0"));
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException();
                }
                response.EnsureSuccessStatusCode(); // This should raise an exception but C# doesn't know this so we'll have to add another Exception, though it shouldn't get reached
                throw new Exception($"Got error code {response.StatusCode} when requesting {uri}"); // This should never actually get reached but let's make it useful just in case
            }
        }

        internal class BadRequestException : Exception
        {
        }
    }
}
