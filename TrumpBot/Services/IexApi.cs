using System;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    public static class IexApi
    {
        public static IexApiModels.IexQuoteApiModel GetIexQuote(string symbol, bool displayPercent = true)
        {
            return JsonConvert.DeserializeObject<IexApiModels.IexQuoteApiModel>(
                Http.GetJson(
                    new Uri($"https://api.iextrading.com/1.0/stock/{symbol}/quote?displayPercent={displayPercent}")));
        }
    }
}