using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    internal class FiveThirtyEightCommands
    {
        [Command.CacheOutput(600)]
        internal class Now : ICommand
        {
            public string CommandName { get; } = "FiveThirtyEight-Now";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^fivethirtyeight [n]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^538 [n]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^538$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^fivethirtyeight$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                FiveThirtyEightPolls.UsPolls usPolls = FiveThirtyEightPolls.GetUsPolls(useCache);
                return new List<string>
                {
                    $"Latest Now Forecast: TRUMP: {Math.Round(usPolls.Forecasts.Latest.Republicans.Models.Now.AdjustedAverage, 2)}% - HILLBOT: {Math.Round(usPolls.Forecasts.Latest.Democrats.Models.Now.AdjustedAverage, 2)}% - Who Cares (Johnson): {Math.Round(usPolls.Forecasts.Latest.Libertarians.Models.Now.AdjustedAverage, 2)}%"
                };
            }
        }

        [Command.CacheOutput(600)]
        internal class Polls : ICommand
        {
            public string CommandName { get; } = "FiveThirtyEight-Polls";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^fivethirtyeight [p]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^538 [p]", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                FiveThirtyEightPolls.UsPolls usPolls = FiveThirtyEightPolls.GetUsPolls(useCache);
                return new List<string>
                {
                    $"Latest Polls-Only Forecast: TRUMP: {Math.Round(usPolls.Forecasts.Latest.Republicans.Models.Polls.AdjustedAverage, 2)}% - PANTSUITBOT: {Math.Round(usPolls.Forecasts.Latest.Democrats.Models.Polls.AdjustedAverage, 2)}% - Nobody Cares (Johnson): {Math.Round(usPolls.Forecasts.Latest.Libertarians.Models.Polls.AdjustedAverage, 2)}%"
                };
            }
        }

        [Command.CacheOutput(600)]
        internal class Plus : ICommand
        {
            public string CommandName { get; } = "FiveThirtyEight-Plus";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^fivethirtyeight [+]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^538 [+]", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                FiveThirtyEightPolls.UsPolls usPolls = FiveThirtyEightPolls.GetUsPolls(useCache);
                return new List<string>
                {
                    $"Latest Polls-Plus Forecast: TRUMP: {Math.Round(usPolls.Forecasts.Latest.Republicans.Models.Plus.AdjustedAverage, 2)}% - SHITINPANTS: {Math.Round(usPolls.Forecasts.Latest.Democrats.Models.Plus.AdjustedAverage, 2)}% - Who the fuck cares (Johnson): {Math.Round(usPolls.Forecasts.Latest.Libertarians.Models.Plus.AdjustedAverage, 2)}%"
                };
            }
        }

        [Command.CacheOutput(600)]
        internal class WinProbPlus : ICommand
        {
            public string CommandName { get; } = "FiveThirtyEight-WinProbPlus";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^538 w [+]$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^fivethirtyeight w [+]$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                FiveThirtyEightPolls.UsMap usMap = FiveThirtyEightPolls.GetUsMap(useCache);
                return new List<string>
                {
                    $"Latest Polls-Plus Win Forecast: TRUMP: {Math.Round(usMap.WinForecasts.LatestForecasts.Republicans.WinForecastModels.Plus.WinProbability, 2)}% - SOILEDPANTSUIT: {Math.Round(usMap.WinForecasts.LatestForecasts.Democrats.WinForecastModels.Plus.WinProbability, 2)}% - Yawn: {Math.Round(usMap.WinForecasts.LatestForecasts.Libertarians.WinForecastModels.Plus.WinProbability, 2)}%"
                };
            }
        }

        [Command.CacheOutput(600)]
        internal class WinProbNow : ICommand
        {
            public string CommandName { get; } = "FiveThirtyEight-WinProbNow";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^538 w$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^538 w n$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^fivethirtyeight w$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^fivethirtyeight w n$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                FiveThirtyEightPolls.UsMap usMap = FiveThirtyEightPolls.GetUsMap(useCache);
                return new List<string>
                {
                    $"Latest Now-Cast Win Forecast: TRUMP: {Math.Round(usMap.WinForecasts.LatestForecasts.Republicans.WinForecastModels.Now.WinProbability, 2)}% - COUGHCOUGHCOUGH: {Math.Round(usMap.WinForecasts.LatestForecasts.Democrats.WinForecastModels.Now.WinProbability, 2)}% - Snore: {Math.Round(usMap.WinForecasts.LatestForecasts.Libertarians.WinForecastModels.Now.WinProbability, 2)}%"
                };
            }
        }

        [Command.CacheOutput(600)]
        internal class WinProbPolls : ICommand
        {
            public string CommandName { get; } = "FiveThirtyEight-WinProbPolls";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^538 w p$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^fivethirtyeight w p$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                FiveThirtyEightPolls.UsMap usMap = FiveThirtyEightPolls.GetUsMap(useCache);
                return new List<string>
                {
                    $"Latest Polls-Only Win Forecast: TRUMP: {Math.Round(usMap.WinForecasts.LatestForecasts.Republicans.WinForecastModels.Polls.WinProbability, 2)}% - *collapses*: {Math.Round(usMap.WinForecasts.LatestForecasts.Democrats.WinForecastModels.Polls.WinProbability, 2)}% - zzz: {Math.Round(usMap.WinForecasts.LatestForecasts.Libertarians.WinForecastModels.Polls.WinProbability, 2)}"
                };
            }
        }
    }
}
