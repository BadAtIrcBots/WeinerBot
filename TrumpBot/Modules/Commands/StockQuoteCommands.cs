using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class StockQuoteCommands
    {
        internal static string FormatTicker(IexApiModels.IexQuoteApiModel ticker)
        {
            var latestUpdate =
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ticker.LatestUpdateEpoch);
            return
                $"{ticker.Symbol} - Latest: {ticker.LatestPrice:N} - Open: {ticker.Open:N} - High: {ticker.High:N} - Low: {ticker.Low:N} - Change: {ticker.Change:N} ({ticker.ChangePercent:P}) - Previous Close: {ticker.Close} - Volume: {ticker.LatestVolume:N0} - Market Cap: {ticker.MarketCap:N0} - PE Ratio: {ticker.PeRatio} - YTD Change: {ticker.YtdChange:P}";
            //$"Latest Update: {latestUpdate.Humanize(false, TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")))}";
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

                var ticker = Services.IexApi.GetIexQuote(symbolName);
                if (ticker.Symbol == null)
                {
                    return new List<string>{"Symbol name either doesn't exist or coudln't parse JSON"};
                }
                return new List<string>{FormatTicker(ticker)};
            }
        }
    }
}