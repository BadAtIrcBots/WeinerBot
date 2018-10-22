using System;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    public static class FoxMidterms
    {
        public static FoxMidtermsModels.FoxData GetMidtermData(bool preferCache = true)
        {
            Uri foxUri = new Uri("https://www.foxnews.com/politics/elections/2018/feeds/election.json");
            string cacheName = foxUri.AbsoluteUri;
            DateTimeOffset cacheExpiration = DateTimeOffset.Now.AddMinutes(5);
            var foxData = Cache.Get<FoxMidtermsModels.FoxData>(cacheName);
            if (foxData != null && preferCache) return foxData;

            foxData = JsonConvert.DeserializeObject<FoxMidtermsModels.FoxData>(Http.Get(foxUri, fuzzUserAgent: true,
                compression: true));
            Cache.Set(cacheName, foxData, cacheExpiration);
            return foxData;
        }
    }
}