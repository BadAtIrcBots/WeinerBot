using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class ElectionModel
    {
        private Election _getElectionData(Uri cnnUri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(cnnUri).Result;
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Election>(response.Content.ReadAsStringAsync().Result);
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotReadyYetException();
                }
                throw new Exception($"Bad HTTP response code, {response.StatusCode}");
            }
        }

        public class NotReadyYetException : Exception { }

        public Election GetElectionData(Uri cnnUri, bool useCache = true)
        {
            if (!useCache) return _getElectionData(cnnUri);

            ObjectCache cache = MemoryCache.Default;
            string cacheObjectName = $"CNN-Election-Data-{cnnUri.AbsolutePath}";

            Election electionData = cache[cacheObjectName] as Election;

            if (electionData != null) return electionData;

            electionData = _getElectionData(cnnUri);
            cache.Set(cacheObjectName, electionData, new DateTimeOffset(DateTime.Now.AddSeconds(30)));
            return electionData;
        }

        private Calendar _getCalendarData()
        {
            Uri cnnUri = new Uri("http://data.cnn.com/ELECTION/2016/full/P.full.json");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(cnnUri).Result;
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Calendar>(response.Content.ReadAsStringAsync().Result);
                }
                throw new Exception($"Bad HTTP status code, {response.StatusCode}");
            }
        }

        public Calendar GetCalendar(bool useCache = true)
        {
            if (!useCache) return _getCalendarData();

            ObjectCache cache = MemoryCache.Default;
            string cacheObjectName = "FullCnnCalendar";

            Calendar calendarData = cache[cacheObjectName] as Calendar;

            if (calendarData != null) return calendarData;

            calendarData = _getCalendarData();
            cache.Set(cacheObjectName, calendarData, new DateTimeOffset(DateTime.Now.AddSeconds(30)));
            return calendarData;
        }
    }

    public class Candidate
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fname")]
        public string FirstName { get; set; }

        [JsonProperty("lname")]
        public string LastName { get; set; }

        [JsonProperty("party")]
        public string Party { get; set; }

        [JsonProperty("winner")]
        public bool Winner { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("pctDecimal")]
        public string Percent { get; set; }

    }

    public class Election
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("code")]
        public string StateCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("pollclose")]
        public long PollCloseEpoch { get; set; }

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("etype")]
        public string ElectionType { get; set; } // S for GOP caucus, R for GOP primary, E for Dem caucus, D for Dem primary... no I don't know why

        [JsonProperty("raceid")]
        public string RaceId { get; set; }

        [JsonProperty("candidates")]
        public List<Candidate> Candidates { get; set; } 

        [JsonProperty("pctsrep")]
        public int PercentageReporting { get; set; }
    }

    public class Calendar
    {
        [JsonProperty("race")]
        public string Race { get; set; }

        [JsonProperty("races")]
        public List<Election> Races { get; set; } 
    }

    public class ElectionCommandData
    {
        public string State { get; set; }
        public List<string> Names { get; set; } 
        public Uri CnnUri { get; set; }
        public Uri ShortUri { get; set; }
    }
}
