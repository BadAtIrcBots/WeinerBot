using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrumpBot.Services;

namespace TrumpBot.Models
{
    public static class FiveThirtyEightPolls
    {
        public class BaseUs
        {
            [JsonProperty("state")]
            public string State { get; set; }
            [JsonProperty("candidate")]
            public List<Candidate> Candidates { get; set; }
        }
        public class UsPolls : BaseUs
        {
            [JsonProperty("forecasts")]
            public Forecasts Forecasts { get; set; }
            [JsonProperty("pollAvg")]
            public List<PollAverage> PollAverages { get; set; }
        }

        public class UsMap : BaseUs
        {
            [JsonProperty("forecasts")]
            public WinForecasts WinForecasts { get; set; }
        }

        public class WinForecasts
        {
            [JsonProperty("latest")]
            public LatestWinForecasts LatestForecasts { get; set; }
        }

        public class LatestWinForecasts
        {
            [JsonProperty("D")]
            public PartyWinForecast Democrats { get; set; }
            [JsonProperty("R")]
            public PartyWinForecast Republicans { get; set; }
            [JsonProperty("L")]
            public PartyWinForecast Libertarians { get; set; }
        }

        public class PartyWinForecast
        {
            [JsonProperty("party")]
            public string PartyName { get; set; }
            [JsonProperty("candidate")]
            public string CandidateName { get; set; }
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("models")]
            public WinForecastModels WinForecastModels { get; set; }
        }

        public class WinForecastModels
        {
            [JsonProperty("plus")]
            public WinForecastModel Plus { get; set; }
            [JsonProperty("now")]
            public WinForecastModel Now { get; set; }
            [JsonProperty("polls")]
            public WinForecastModel Polls { get; set; }
        }

        public class WinForecastModel
        {
            [JsonProperty("winprob")]
            public float WinProbability { get; set; }

            [JsonProperty("forecast")]
            public float Forecast { get; set; }

            [JsonProperty("hi")]
            public float High { get; set; }

            [JsonProperty("lo")]
            public float Low { get; set; }
        }

        public class PollAverage
        {
            [JsonProperty("party")]
            public string Partyname { get; set; }
            [JsonProperty("candidate")]
            public string CandidateName { get; set; }
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("models")]
            public ShortModels ShortModels { get; set; }
        }

        public class ShortModels
        {
            [JsonProperty("plus")]
            public float Plus { get; set; }

            [JsonProperty("now")]
            public float Now { get; set; }

            [JsonProperty("polls")]
            public float Polls { get; set; }
        }

        public class Forecasts
        {
            [JsonProperty("latest")]
            public Latest Latest { get; set; }
        }

        public class Latest
        {
            [JsonProperty("D")]
            public Party Democrats { get; set; }
            [JsonProperty("R")]
            public Party Republicans { get; set; }
            [JsonProperty("L")]
            public Party Libertarians { get; set; }
        }

        public class Party
        {
            [JsonProperty("party")]
            public string PartyName { get; set; }
            [JsonProperty("candidate")]
            public string CandidateName { get; set; }
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("models")]
            public Models Models { get; set; }
        }

        public class Models
        {
            [JsonProperty("plus")]
            public PollModelData Plus { get; set; }
            [JsonProperty("polls")]
            public PollModelData Polls { get; set; }
            [JsonProperty("now")]
            public PollModelData Now { get; set; }
        }

        public class PollModelData
        {
            [JsonProperty("poll_avg")]
            public float Average { get; set; }
            [JsonProperty("likely_adjust")]
            public float? LikelyAdjust { get; set; }
            [JsonProperty("convention_adjust")]
            public float? ConventionAdjust { get; set; }
            [JsonProperty("vp_adjust")]
            public float? VpAdjust { get; set; }
            [JsonProperty("third_adjust")]
            public float? ThirdAdjust { get; set; }
            [JsonProperty("trend_adjust")]
            public float? TrendAdjust { get; set; }
            [JsonProperty("house_adjust")]
            public float? HouseAdjust { get; set; }
            [JsonProperty("adj_avg")]
            public float AdjustedAverage { get; set; }

        }

        public class Candidate
        {
            [JsonProperty("candidate")]
            public string CandidateName { get; set; }
            [JsonProperty("party")]
            public string PartyName { get; set; }
        }

        public static UsPolls GetUsPolls(bool useCache = true)
        {
            Uri fiveThirtyEightUri = new Uri("http://projects.fivethirtyeight.com/2016-election-forecast/US-polls.json");
            if (!useCache)
            {
                return _getUsPolls(fiveThirtyEightUri);
            }
            string cacheObjectName = "FiveThirtyEight-UsPolls";
            ObjectCache cache = MemoryCache.Default;
            UsPolls usPolls = cache[cacheObjectName] as UsPolls;
            if(usPolls != null) return usPolls;
            usPolls = _getUsPolls(fiveThirtyEightUri);
            cache.Set(cacheObjectName, usPolls, new DateTimeOffset(DateTime.Now.AddMinutes(1)));
            return usPolls;
        }

        public static UsMap GetUsMap(bool useCache = true)
        {
            Uri mapUri = new Uri("http://projects.fivethirtyeight.com/2016-election-forecast/US.json");
            if (!useCache)
            {
                return _getUsMap(mapUri);
            }
            string cacheObjectName = "FiveThirtyEight-UsMap";
            ObjectCache cache = MemoryCache.Default;
            UsMap usMap = cache[cacheObjectName] as UsMap;
            if (usMap != null) return usMap;
            usMap = _getUsMap(mapUri);
            cache.Set(cacheObjectName, usMap, new DateTimeOffset(DateTime.Now.AddMinutes(1)));
            return usMap;
        }

        private static UsPolls _getUsPolls(Uri uri)
        {
            string result = Http.GetJson(uri, fuzzUserAgent: true, acceptHeader: "text/html", compression: true);
            return JsonConvert.DeserializeObject<UsPolls>(result);
        }

        private static UsMap _getUsMap(Uri uri)
        {
            string result = Http.GetJson(uri, fuzzUserAgent: true, acceptHeader: "text/html", compression: true);
            return JsonConvert.DeserializeObject<UsMap>(result);
        }
    }
}
