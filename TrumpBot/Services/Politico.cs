using System;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    public static class Politico
    {
        public static PoliticoMidtermsModels.ResultsRootModel GetMidtermResults(bool preferCache = true)
        {
            Uri politicoUri = new Uri("https://www.politico.com/election-results/2018/balance-of-power.json");
            string cacheName = politicoUri.AbsoluteUri;
            DateTimeOffset cacheExpiration = DateTimeOffset.Now.AddSeconds(30);
            var politicoData = Cache.Get<PoliticoMidtermsModels.ResultsRootModel>(cacheName);
            if (politicoData != null && preferCache) return politicoData;

            politicoData = JsonConvert.DeserializeObject<PoliticoMidtermsModels.ResultsRootModel>(Http.Get(politicoUri, fuzzUserAgent: true,
                compression: true));
            Cache.Set(cacheName, politicoData, cacheExpiration);
            return politicoData;
        }
    }
}