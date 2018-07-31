using System;
using Newtonsoft.Json;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Services
{
    public static class AlphaVantageApi
    {
        public static AlphaVantageApiModels.DailyTimeSeriesModel.TimeSeriesDailyApiModel GetTimeSeriesDaily(
            string symbol, string outputSize = "compact")
        {
            AlphaVantageApiConfigModel config =
                ConfigHelpers.LoadConfig<AlphaVantageApiConfigModel>(ConfigHelpers.ConfigPaths.AlphaVantageApiConfig);
            return JsonConvert.DeserializeObject<AlphaVantageApiModels.DailyTimeSeriesModel.TimeSeriesDailyApiModel>(
                Http.GetJson(new Uri(
                    $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={config.ApiKey}&outputsize={outputSize}")));
        }
    }
}