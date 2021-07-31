﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WeinerBot.Extensions;
using WeinerBot.Models;
using WeinerBot.Services;

namespace WeinerBot.Modules.Commands
{
    [Command.CacheOutput(15)]
    internal class GdaxTickerCommand : ICommand
    {
        public string CommandName { get; } = "Get GDAX Ticker";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^gdax$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; }

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            GdaxTickerApiModel ticker =
                JsonConvert.DeserializeObject<GdaxTickerApiModel>(
                    Http.Get(new Uri("https://api.gdax.com/products/BTC-USD/stats"), compression: true, fuzzUserAgent: true));

            return $"GDAX: Last: ${ticker.Last:n2} - High: ${ticker.High:n2} - Low: ${ticker.Low:n2} - Volume: {ticker.Volume:n2} BTC".SplitInParts(430).ToList();
        }
    }
    
    [Command.CacheOutput(15)]
    internal class GdaxBcashTickerCommand : ICommand
    {
        public string CommandName { get; } = "Get GDAX BitCoin Cash Ticker";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^gdax bch$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^gdax bcash$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; }

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            GdaxTickerApiModel ticker =
                JsonConvert.DeserializeObject<GdaxTickerApiModel>(
                    Http.Get(new Uri("https://api.gdax.com/products/BCH-USD/stats"), compression: true, fuzzUserAgent: true));

            return $"GDAX BCH: Last: ${ticker.Last:n2} - High: ${ticker.High:n2} - Low: ${ticker.Low:n2} - Volume: {ticker.Volume:n2} BTC".SplitInParts(430).ToList();
        }
    }
}