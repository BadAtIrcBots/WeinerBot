using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TrumpBot.Services
{
    internal class Http
    {
        internal static string GetJson(Uri uri, bool fuzzUserAgent = false, string acceptHeader = "application/json", bool compression = false)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (compression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
                if (fuzzUserAgent)
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.122 Safari/537.36 Vivaldi/1.4.589.29");
                }

                HttpResponseMessage response = client.GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }

                throw new HttpException($"Bad HTTP response code: {response.StatusCode}", response.StatusCode);
            }
        }

        internal static string Get(Uri uri, bool fuzzUserAgent = false, string acceptHeader = "text/html",
            bool compression = false, int timeout = 5000)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (compression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
                client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                if (fuzzUserAgent)
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.122 Safari/537.36 Vivaldi/1.4.589.29");
                }

                HttpResponseMessage response = client.GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                throw new HttpException($"Bad HTTP response code: {response.StatusCode}", response.StatusCode);
            }
        }

        internal class HttpException : Exception
        {
            public HttpStatusCode Code;

            public HttpException(string message, HttpStatusCode code) : base(message)
            {
                Code = code;
            }
        }
    }
}
