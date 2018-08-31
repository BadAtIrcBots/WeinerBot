using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    public class StockQuoteCommands
    {
        internal static string FormatTicker(IexApiModels.IexQuoteApiModel ticker)
        {
            var b = IrcConstants.IrcBold;
            var n = IrcConstants.IrcNormal;
            var c = IrcConstants.IrcColor;
            var colours = new Colours();
            var changeColour = colours.Green;
            if (ticker.Change < 0)
            {
                changeColour = colours.LightRed;
            }

            var ytdChangeColour = colours.Green;
            if (ticker.YtdChange < 0)
            {
                ytdChangeColour = colours.LightRed;
            }

            var peRatioColour = colours.Green;
            if (ticker.PeRatio < 0)
            {
                peRatioColour = colours.LightRed;
            }
            return
                $"{b}{ticker.Symbol}{n} ({ticker.CompanyName}) - " +
                $"{b}Latest{n}: {ticker.LatestPrice:N} - " +
                $"{b}Open{n}: {ticker.Open:N} - " +
                $"{b}High{n}: {ticker.High:N} - " +
                $"{b}Low{n}: {ticker.Low:N} - " +
                $"{b}Change{n}: {c}{changeColour}{ticker.Change:N}{n} ({c}{changeColour}{ticker.ChangePercent:P}{n}) - " +
                $"{b}Previous Close{n}: {ticker.Close} - " +
                $"{b}Volume{n}: {ticker.LatestVolume:N0} - " +
                $"{b}Market Cap{n}: ${ticker.MarketCap:N0} - " +
                $"{b}PE Ratio{n}: {c}{peRatioColour}{ticker.PeRatio}{n} - " +
                $"{b}YTD Change{n}: {c}{ytdChangeColour}{ticker.YtdChange:P}{n} - " +
                $"{b}52 Week Low/High{n}: {ticker.Week52Low:N}/{ticker.Week52High:N}";
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
                IexApiModels.IexQuoteApiModel ticker;

                try
                {
                    ticker = Services.IexApi.GetIexQuote(symbolName);
                }
                catch (Http.HttpException e)
                {
                    if (e.Message.Contains("NotFound"))
                    {
                        return new List<string> {"Symbol does not exist"};
                    }

                    throw;
                }
                
                if (ticker.Symbol == null)
                {
                    return new List<string>{"JSON is all fucked up."};
                }
                return new List<string>{FormatTicker(ticker)};
            }
        }
    }
}