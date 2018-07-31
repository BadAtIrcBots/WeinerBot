using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class StockQuoteCommands
    {
        internal static string FormatTicker(AlphaVantageApiModels.DailyTimeSeriesModel.DayModel ticker, string symbol, string date)
        {
            return $"{symbol.ToUpper()} ({date}) - Open: {ticker.Open} - High: {ticker.High} - Low: {ticker.Low} - Close: {ticker.Close} - Volume: {ticker.Volume}";
        }

        internal class GetLatestDailyQuoteBySymbol : ICommand
        {
            public string CommandName { get; } = "GetLatestDailyQuoteBySymbol";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^stock (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                if (arguments == null || arguments.Count == 1)
                {
                    throw new ArgumentException("Not enough arguments");
                }

                string symbolName = arguments[1].Value;

                var ticker = Services.AlphaVantageApi.GetTimeSeriesDaily(symbolName);
                if (ticker.MetaData == null)
                {
                    return new List<string>{"Ticker doesn't exist or couldn't parse the JSON"};
                }
                var latest = ticker.TimeSeries.Keys.OrderByDescending(DateTime.Parse).First();
                return new List<string>{FormatTicker(ticker.TimeSeries[latest], symbolName, latest)};
            }
        }
    }
}