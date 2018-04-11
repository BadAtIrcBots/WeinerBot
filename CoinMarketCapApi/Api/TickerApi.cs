using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoinMarketCapApi.Exceptions;
using CoinMarketCapApi.Models;
using Newtonsoft.Json;

namespace CoinMarketCapApi.Api
{
    public static class TickerApi
    {
        public static async Task<List<TickerModel>> GetTicker(string tickerName)
        {
            Uri tickerUri = new Uri($"https://api.coinmarketcap.com/v1/ticker/{tickerName}/");

            HttpClientHandler handler =
                new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CoinMarketCapApi", "1.0.0.0"));
                HttpResponseMessage response = await client.GetAsync(tickerUri);
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<TickerModel>>(await response.Content.ReadAsStringAsync());
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new TickerNotFoundException();
                }
                response.EnsureSuccessStatusCode();
                throw new Exception(); // Above code should ensure the proper exception is raised, this is just so the compiler knows what's up
            }
        }

        public static async Task<List<TickerModel>> GetAllTickers(int limit = 100)
        {
            Uri tickerUri = new Uri("https://api.coinmarketcap.com/v1/ticker/?limit=" + limit);

            HttpClientHandler handler =
                new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CoinMarketCapApi", "1.0.0.0"));
                HttpResponseMessage response = await client.GetAsync(tickerUri);
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<TickerModel>>(await response.Content.ReadAsStringAsync());
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new TickerNotFoundException();
                }
                response.EnsureSuccessStatusCode();
                throw new Exception(); // Above code should ensure the proper exception is raised, this is just so the compiler knows what's up
            }
        }
    }
}
