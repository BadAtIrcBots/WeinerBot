using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class DelegateModel
    {
        public class NationalRace
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("rd")]
            public int RegisteredDelegates { get; set; }

            [JsonProperty("pd")]
            public int PledgedDelegates { get; set; }

            [JsonProperty("td")]
            public int TotalDelegates { get; set; }

            [JsonProperty("winner")]
            public bool Winner { get; set; }
        }

        public class Candidate
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("fname")]
            public string FirstName { get; set; }

            [JsonProperty("lname")]
            public string LastName { get; set; }

            [JsonProperty("nominee")]
            public bool Nominee { get; set; }
        }

        public class State
        {
            [JsonProperty("state")]
            public string StateName { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("electiondate")]
            public string ElectionDate { get; set; }

            [JsonProperty("primarytype")]
            public string PrimaryType { get; set; }

            [JsonProperty("pctsrep")]
            public int? PercentRepresented { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("candidates")]
            public List<StateCandidate> Candidates { get; set; }
        }

        public class StateCandidate
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("rd")]
            public int RegisteredDelegates { get; set; }

            [JsonProperty("pd")]
            public int PledgedDelegates { get; set; }

            [JsonProperty("td")]
            public int TotalDelegates { get; set; }

            [JsonProperty("vpct")]
            public int Percent { get; set; }

            [JsonProperty("pctDecimal")]
            public string DecimalPercent { get; set; }

            [JsonProperty("winner")]
            public bool Winner { get; set; }
        }

        public class DelegateStatistics
        {
            [JsonProperty("party")]
            public string Party { get; set; }

            [JsonProperty("d_nom")]
            public int RequiredDelegates { get; set; }

            [JsonProperty("td_k")]
            public int TotalDelegates { get; set; }

            [JsonProperty("td")]
            public string AllocatedDelegates { get; set; }

            [JsonProperty("ts")]
            public long Timestamp { get; set; }

            [JsonProperty("nationraces")]
            public List<NationalRace> NationalRaces { get; set; } 

            [JsonProperty("candidates")]
            public List<Candidate> Candidates { get; set; } 

            [JsonProperty("states")]
            public List<State> States { get; set; } 
        }

        private DelegateStatistics _getDelegateStatistics(Uri cnnUri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(cnnUri).Result;
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<DelegateStatistics>(response.Content.ReadAsStringAsync().Result);
                }
                throw new Exception($"Bad HTTP response code, {response.StatusCode}");
            }
        }

        public DelegateStatistics GetDelegateStatistics(Uri cnnUri, bool useCache = true)
        {
            if (!useCache) return _getDelegateStatistics(cnnUri);

            ObjectCache cache = MemoryCache.Default;

            string cacheObjectName = $"CNN-Delegate-Data-{cnnUri.AbsolutePath}";

            DelegateStatistics delegateStatistics = cache[cacheObjectName] as DelegateStatistics;
            
            if(delegateStatistics != null) return delegateStatistics;

            delegateStatistics = _getDelegateStatistics(cnnUri);
            cache.Set(cacheObjectName, delegateStatistics, new DateTimeOffset(DateTime.Now.AddMinutes(5)));

            return delegateStatistics;
        }
    }
}
