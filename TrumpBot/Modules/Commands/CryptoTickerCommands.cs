using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Humanizer;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class CryptoTickerCommands
    {
        public class GetFinexPubTickerCommand : ICommand
        {
            public string CommandName { get; } = "Get Bitfinex Pub Ticker";

            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^fin$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^finex$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetch Bitfinex pub ticker data for BTCUSD pair";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                var tickerData = Services.CryptoTickers.GetFinexPubTicker();
                return new List<string> {$"Finex - Last: {tickerData.LastPrice:N} - High: {tickerData.High:N} - " +
                                         $"Low: {tickerData.Low:N} - Volume: {tickerData.Volume:N} - " +
                                         $"Last Updated: {DateTime.UtcNow.Humanize(true, tickerData.Timestamp)}"};
            }
        }
    }
}