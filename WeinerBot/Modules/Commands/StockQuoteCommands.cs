using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using WeinerBot.Models;
using WeinerBot.Services;

namespace WeinerBot.Modules.Commands
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
                $"{b}52 Week Low/High{n}: {ticker.Week52Low:N}/{ticker.Week52High:N} - " +
                $"{b}Updated{n}: {ticker.LatestTime}";
        }

        internal static string FormatAfterHoursTicker(IexApiModels.IexQuoteApiModel ticker)
        {
            var b = IrcConstants.IrcBold;
            var n = IrcConstants.IrcNormal;
            var c = IrcConstants.IrcColor;
            var colours = new Colours();
            var changeColour = colours.Green;
            if (ticker.ExtendedChange < 0)
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
                $"{b}Latest (AH){n}: {ticker.ExtendedPrice:N} - " +
                $"{b}Open{n}: {ticker.Open:N} - " +
                $"{b}Close{n}: {ticker.Close:N} - " +
                $"{b}High{n}: {ticker.High:N} - " +
                $"{b}Low{n}: {ticker.Low:N} - " +
                $"{b}Change (since close){n}: {c}{changeColour}{ticker.ExtendedChange:N}{n} ({c}{changeColour}{ticker.ExtendedChangePercent:P}{n}) - " +
                $"{b}Volume{n}: {ticker.LatestVolume:N0} - " +
                $"{b}Market Cap{n}: ${ticker.MarketCap:N0} - " +
                $"{b}PE Ratio{n}: {c}{peRatioColour}{ticker.PeRatio}{n} - " +
                $"{b}YTD Change{n}: {c}{ytdChangeColour}{ticker.YtdChange:P}{n} - " +
                $"{b}52 Week Low/High{n}: {ticker.Week52Low:N}/{ticker.Week52High:N}";
        }

        internal class GetLatestDailyQuoteBySymbol : ICommand
        {
            public string CommandName { get; } = "Get Stock Quote";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^stock (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Get a stock quote for a given symbol. This uses the IEX Cloud API which supports extended trading hours and international exchanges.";
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
                    ticker = IexApi.GetIexQuote(symbolName);
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

                DateTimeOffset now = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time").BaseUtcOffset);
                DateTimeOffset openTime = new DateTimeOffset(now.Year, now.Month, now.Day, 9, 30, 0, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time").BaseUtcOffset); // Even though it says standard time it is actually DST aware
                DateTimeOffset closeTime = new DateTimeOffset(now.Year, now.Month, now.Day, 16, 0, 0, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time").BaseUtcOffset);
                if ((now >= openTime && now <= closeTime) && !(now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday))
                {
                    return new List<string>{FormatTicker(ticker)};
                }
                return new List<string>{FormatAfterHoursTicker(ticker)};
            }
        }

        [Command.BreakAfterExecution]
        internal class GetTslaDailyQuote : ICommand
        {
            public string CommandName { get; } = "Get TSLA Quote";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^tsla$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.High;
            public bool HideFromHelp { get; set; } = true;
            public string HelpDescription { get; set; } = "Shortcut for GetLatestDailyQuoteBySymbol";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                GroupCollection newCollection =
                    new Regex(@"^stock (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match("stock tsla").Groups;
                return new GetLatestDailyQuoteBySymbol().RunCommand(messageEvent, newCollection, useCache);
            }
        }
    }
}