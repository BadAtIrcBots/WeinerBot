using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class PollChart
    {
        private readonly Uri _chartsApiUri = new Uri("http://elections.huffingtonpost.com/pollster/api/charts");
        private List<Chart> _getCharts(string topic)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = _chartsApiUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync($"?topic={topic}").Result;
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Chart>>(response.Content.ReadAsStringAsync().Result);
                }
                throw new Exception($"Bad HTTP response code, {response.StatusCode}");
            }
        }

        public List<Chart> GetCharts(string topic, bool useCache = true)
        {
            if(!useCache) return _getCharts(topic);

            ObjectCache cache = MemoryCache.Default;
            string cacheObjectName = $"PollsterChart-topic={topic}";
            List<Chart> charts = cache[cacheObjectName] as List<Chart>;

            if (charts != null) return charts;

            charts = _getCharts(topic);
            cache.Set(cacheObjectName, charts, new DateTimeOffset(DateTime.Now.AddMinutes(30)));

            return charts;
        } 
    }

    public class Chart
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("short_title")]
        public string ShortTitle { get; set; }
        [JsonProperty("election_date")]
        public DateTime? ElectionDate { get; set; } // Date is a YYYY-MM-dd, but it is sometimes null hence it can't be converted to DateTime
        [JsonProperty("poll_count")]
        public int PollCount { get; set; }
        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }
        [JsonProperty("url")]
        public Uri PollUri { get; set; }
        [JsonProperty("estimates")]
        public List<Estimate> Estimates { get; set; }
    }

    public class Estimate
    {
        [JsonProperty("choice")]
        public string Choice { get; set; }
        [JsonProperty("value")]
        public float Value { get; set; }
        [JsonProperty("lead_confidence")]
        public float? LeadConfidence { get; set; } // This value is null at the moment, I have no idea what it should be
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; } // TRUMP!
        [JsonProperty("party")]
        public string Party { get; set; }
        [JsonProperty("incumbent")]
        public bool? Incumbent { get; set; }
    }
}
