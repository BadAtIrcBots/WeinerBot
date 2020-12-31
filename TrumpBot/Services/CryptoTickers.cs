using System;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    public class CryptoTickers
    {
        public static CryptoTickerModels.FinexTickerModel GetFinexPubTicker(string symbol = "btcusd")
        {
            return JsonConvert.DeserializeObject<CryptoTickerModels.FinexTickerModel>(
                Http.GetJson(new Uri($"https://api.bitfinex.com/v1/pubticker/{symbol}")));
        }
    }
}