using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Newtonsoft.Json;
using TrumpBot.Services;

namespace TrumpBot.Models
{
    public class FranceElectionApiModel
    {
        private List<Election> _getElectionData(Uri uri)
        {
            string response = Http.GetJson(uri, fuzzUserAgent: true, compression: true);

            return JsonConvert.DeserializeObject<List<Election>>(response);
        }

        public List<Election> GetElectionData(bool useCache = true)
        {
            Uri uri = new Uri("https://www.francetvinfo.fr/resultats/resultats/all/presidential/national/france.json");

            if (!useCache) return _getElectionData(uri);

            ObjectCache cache = MemoryCache.Default;
            string cacheObjectName = "France-Election-Data";

            List<Election> electionData = cache[cacheObjectName] as List<Election>;

            if (electionData != null) return electionData;

            electionData = _getElectionData(uri);
            cache.Set(cacheObjectName, electionData, new DateTimeOffset(DateTime.Now.AddSeconds(30)));
            return electionData;
        }

        public class Number
        {
            [JsonProperty("percent")]
            public double? Percent { get; set; }
            [JsonProperty("votes")]
            public int? Votes { get; set; }
        }

        public class VoteInfo
        {
            [JsonProperty("votes")]
            public int? Votes { get; set; }
            [JsonProperty("percent")]
            public double? Percent { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("label")]
            public string Label { get; set; } // Usually blank
        }

        public class Election
        {
            [JsonProperty("insee")]
            public string Country { get; set; }
            [JsonProperty("year")]
            public int Year { get; set; }
            [JsonProperty("round")]
            public int Round { get; set; }
            [JsonProperty("estimate")]
            public bool Estimate { get; set; }
            [JsonProperty("updated_at")]
            public long UpdatedAt { get; set; } // Epoch
            [JsonProperty("sources")]
            public List<string> Sources { get; set; }
            [JsonProperty("population")]
            public long Population { get; set; }
            [JsonProperty("registered")]
            public long? Registered { get; set; }
            
            [JsonProperty("participation")]
            public Number Participation { get; set; }
            [JsonProperty("abstained")]
            public Number Abstained { get; set; }
            [JsonProperty("real")]
            public Number Real { get; set; }
            [JsonProperty("blank_and_nul")]
            public Number BlankAndNul { get; set; }
            [JsonProperty("blank")]
            public Number Blank { get; set; }
            [JsonProperty("nul")]
            public Number Nul { get; set; }
            [JsonProperty("votes")]
            public List<VoteInfo> Votes { get; set; }
        }
    }
}
