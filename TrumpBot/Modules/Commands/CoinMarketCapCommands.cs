using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CoinMarketCapApi.Models;
using Meebey.SmartIrc4net;
using TrumpBot.Configs;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    internal class CoinMarketCapCommands
    {
        internal static class CoinMarketCapHelpers
        {
            internal static string FormatPercentChange(double percentage)
            {
                var colours = new Colours();
                string c = IrcConstants.IrcColor.ToString();

                if (percentage < -10)
                {
                    c += colours.LightRed;
                }
                else if (percentage < -7.5)
                {
                    c += colours.Orange;
                }
                else if (percentage < 0)
                {
                    c += colours.Yellow + "," + colours.Grey;
                }
                else if (percentage < 10)
                {
                    c += colours.LightGreen;
                }
                else
                {
                    c += colours.Green;
                }
                c += IrcConstants.IrcBold.ToString() + percentage + "%" + IrcConstants.IrcNormal;
                return c;
            }

            internal static List<string> FormatTicker(TickerModel ticker)
            {
                var b = IrcConstants.IrcBold;
                var n = IrcConstants.IrcNormal;

                DateTime updateDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(ticker.LastUpdated));
                
                // I think I'm gonna have an aneurysm, why is there no format option where I can keep all my decimal places? Fuck you C#
                int decimalPlaces = ticker.PriceUsd.Split('.').Last().Length;

                string response =
                    $"{b}{ticker.TickerName} ({ticker.Symbol}):{n} " +
                    $"{b}Price:{n} ${Convert.ToDouble(ticker.PriceUsd).ToString("N" + decimalPlaces)} / {ticker.PriceBtc} BTC; {b}Volume (24h):{n} {Convert.ToDecimal(ticker.DayVolumeUsd):n0}; " +
                    $"{b}“Market Cap”:{n} ${Convert.ToDecimal(ticker.MarketCapUsd):n0}; " +
                    $"{b}Change:{n} 1h: {FormatPercentChange(Convert.ToDouble(ticker.PercentChange1Hour))}, 24h: {FormatPercentChange(Convert.ToDouble(ticker.PercentChange24Hours))}, 7d: {FormatPercentChange(Convert.ToDouble(ticker.PercentChange7Days))}; " +
                    $"{b}Last Updated:{n} {(int) (DateTime.UtcNow - updateDate).TotalMinutes} minutes ago";

                return response.SplitInParts(430).ToList();
            }
        }

        [Command.CacheOutput(30)]
        internal class GetBitcoinCashTicker : ICommand
        {
            public string CommandName { get; } = "GetBitcoinCashTicker";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex("^bcc$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex("^bch$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                const string tickerName = "bitcoin-cash";
                TickerModel ticker;
                try
                {
                    ticker = CoinMarketCapApi.Api.TickerApi.GetTicker(tickerName).Result[0];
                }
                catch (Exception e)
                {
                    if (e.InnerException is CoinMarketCapApi.Exceptions.TickerNotFoundException)
                    {
                        return "Ticker not found".SplitInParts(430).ToList();
                    }
                    throw;
                }

                return CoinMarketCapHelpers.FormatTicker(ticker);
            }
        }
        
        [Command.CacheOutput(30)]
        internal class GetBitcoinTicker : ICommand
        {
            public string CommandName { get; } = "GetBitcoinTicker";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex("^btc$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                const string tickerName = "bitcoin";
                TickerModel ticker;
                try
                {
                    ticker = CoinMarketCapApi.Api.TickerApi.GetTicker(tickerName).Result[0];
                }
                catch (Exception e)
                {
                    if (e.InnerException is CoinMarketCapApi.Exceptions.TickerNotFoundException)
                    {
                        return "Ticker not found".SplitInParts(430).ToList();
                    }
                    throw;
                }

                return CoinMarketCapHelpers.FormatTicker(ticker);
            }
        }

        internal class GetGenericTicker : ICommand
        {
            public string CommandName { get; } = "GetGenericTicker";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^coin (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent,
                GroupCollection arguments = null, bool useCache = true)
            {
                if (arguments == null || arguments.Count == 1)
                {
                    throw new ArgumentException("Not enough arguments");
                }
                string tickerName = arguments[1].Value;

                List<CoinMarketSymbolCacheModel> symbolCache = Cache.Get<List<CoinMarketSymbolCacheModel>>("CoinMarketSymbolCache");

                if (symbolCache == null)
                {
                    // 0 means all in CoinMarketCap-land
                    List<TickerModel> symbolList = CoinMarketCapApi.Api.TickerApi.GetAllTickers(limit: 0).Result;

                    symbolCache = symbolList.Select(symbol =>
                        new CoinMarketSymbolCacheModel {Name = symbol.TickerId, Symbol = symbol.Symbol}).ToList();

                    Cache.Set("CoinMarketSymbolCache", symbolCache, DateTimeOffset.Now.AddDays(1));
                }

                foreach (CoinMarketSymbolCacheModel symbol in symbolCache)
                {
                    if (!String.Equals(tickerName, symbol.Symbol, StringComparison.CurrentCultureIgnoreCase)) continue;
                    tickerName = symbol.Name;
                    break;
                }
                
                TickerModel ticker;
                try
                {
                    ticker = CoinMarketCapApi.Api.TickerApi.GetTicker(tickerName).Result[0];
                }
                catch (Exception e)
                {
                    if (e.InnerException is CoinMarketCapApi.Exceptions.TickerNotFoundException)
                    {
                        return "Ticker not found".SplitInParts(430).ToList();
                    }
                    throw;
                }

                return CoinMarketCapHelpers.FormatTicker(ticker);
            }
        }

        internal class GetRandomTicker : ICommand
        {
            public string CommandName { get; } = "GetRandomTicker";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex("^coin$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                List<CoinMarketSymbolCacheModel> symbolCache = Cache.Get<List<CoinMarketSymbolCacheModel>>("CoinMarketSymbolCache");

                if (symbolCache == null)
                {
                    // 0 means all in CoinMarketCap-land
                    List<TickerModel> symbolList = CoinMarketCapApi.Api.TickerApi.GetAllTickers(limit: 0).Result;

                    symbolCache = symbolList.Select(symbol =>
                        new CoinMarketSymbolCacheModel {Name = symbol.TickerId, Symbol = symbol.Symbol}).ToList();

                    Cache.Set("CoinMarketSymbolCache", symbolCache, DateTimeOffset.Now.AddDays(1));
                }

                string tickerName = symbolCache[new Random().Next(0, symbolCache.Count - 1)].Name;
                
                TickerModel ticker;
                try
                {
                    ticker = CoinMarketCapApi.Api.TickerApi.GetTicker(tickerName).Result[0];
                }
                catch (Exception e)
                {
                    if (e.InnerException is CoinMarketCapApi.Exceptions.TickerNotFoundException)
                    {
                        return "Ticker not found".SplitInParts(430).ToList();
                    }
                    throw;
                }

                return CoinMarketCapHelpers.FormatTicker(ticker);
            }
        }

        internal class GetTetherTicker : ICommand
        {
            public string CommandName { get; } = "GetTetherTicker";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^usdt$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^tether$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                TickerModel ticker = CoinMarketCapApi.Api.TickerApi.GetTicker("tether").Result[0];
                TetherConfigModel tetherConfigModel =
                    new BaseConfig().LoadConfig<TetherConfigModel>("Config\\tether_config.json");
                return $"{ticker.TickerName}: Total Supply: {Convert.ToDecimal(ticker.TotalSupply):n0} - Available Supply: {Convert.ToDecimal(ticker.AvailableSupply):n0} (-{(Convert.ToDecimal(ticker.TotalSupply) - Convert.ToDecimal(ticker.AvailableSupply)):n0}) - Last issued: ~{(DateTime.UtcNow - tetherConfigModel.LastChange).TotalHours:n2} hours ago".SplitInParts(430).ToList();
            }
        }

        internal class SearchTickers : ICommand
        {
            public string CommandName { get; } = "SearchTicker";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^coinsearch (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                List<CoinMarketSymbolCacheModel> symbolCache = Cache.Get<List<CoinMarketSymbolCacheModel>>("CoinMarketSymbolCache");

                if (symbolCache == null)
                {
                    // 0 means all in CoinMarketCap-land
                    List<TickerModel> symbolList = CoinMarketCapApi.Api.TickerApi.GetAllTickers(limit: 0).Result;

                    symbolCache = symbolList.Select(symbol =>
                        new CoinMarketSymbolCacheModel {Name = symbol.TickerId, Symbol = symbol.Symbol}).ToList();

                    Cache.Set("CoinMarketSymbolCache", symbolCache, DateTimeOffset.Now.AddDays(1));
                }

                var results = "";
                if (arguments == null || arguments.Count == 1)
                {
                    throw new ArgumentException("Not enough arguments");
                }

                Regex searchRegex;

                try
                {
                    searchRegex = new Regex(arguments[1].Value, RegexOptions.IgnoreCase);
                }
                catch (ArgumentException e)
                {
                    return new List<string>{"Regex was invalid"};
                }

                int matches = 0;

                foreach (var symbol in symbolCache)
                {
                    if (matches > 50)
                    {
                        results += "reached limit, search better moron";
                        break;
                    };
                    if (searchRegex.Match(symbol.Name).Success || searchRegex.Match(symbol.Symbol).Success)
                    {
                        matches++;
                        results += $"{symbol.Name} ({symbol.Symbol}); ";
                    }
                }

                if (results == "")
                {
                    return "Lol no results".SplitInParts(430).ToList();
                }

                return results.SplitInParts(430).ToList();
            }
        }
    }
}
