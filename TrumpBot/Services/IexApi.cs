using System;
using Newtonsoft.Json;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Services
{
    public static class IexApi
    {
        public static IexApiModels.IexQuoteApiModel GetIexQuote(string symbol, bool displayPercent = true)
        {
            var config = ConfigHelpers.LoadConfig<IexApiConfigModel>(ConfigHelpers.ConfigPaths.IexApiConfig);
            if (string.IsNullOrEmpty(config.ApiToken))
            {
                throw new Exception("IEX API token is null");
            }
            return JsonConvert.DeserializeObject<IexApiModels.IexQuoteApiModel>(
                Http.GetJson(
                    new Uri($"https://cloud.iexapis.com/beta/stock/{symbol}/quote?displayPercent={displayPercent}&token={config.ApiToken}")));
        }
    }
}