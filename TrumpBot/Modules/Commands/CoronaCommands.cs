using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Humanizer;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    internal class CoronaCommands
    {
        internal class GetCoronaWorldStats : ICommand
        {
            public string CommandName { get; } = "Get Coronavirus World Stats";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^coron\S+$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^viru\S+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetch worldwide Coronavirus stats";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                GroupCollection newCollection =
                    new Regex(@"^coron\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match("corona total").Groups;
                return new GetCoronaRegionStats().RunCommand(messageEvent, newCollection, useCache);
            }
        }

        internal class GetCoronaMultipleRegionStats : ICommand
        {
            public string CommandName { get; } = "Get Coronavirus Multiple Region Stats";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^coron\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^viru\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Get Coronavirus stats for multiple regions, comma separated";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                if (!arguments[1].Value.Contains(","))
                {
                    return null;
                }
                string[] regions = arguments[1].Value.ToLower().Split(',');
                var result = new List<string>();

                foreach (var region in regions)
                {
                    GroupCollection newCollection =
                        new Regex(@"^coron\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match($"corona {region.TrimStart(' ').TrimEnd(' ')}").Groups;
                    result.Add(new GetCoronaRegionStats().RunCommand(messageEvent, newCollection, useCache)[0]);
                }

                return result;
            }
        }

        internal class GetCoronaRegionStats : ICommand
        {
            public string CommandName { get; } = "Get Coronavirus Region Stats";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^coron\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^viru\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Get Coronavirus stats by region";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                if (arguments[1].Value.Contains(","))
                {
                    return null;
                }
                string country = arguments[1].Value.ToLower();
                string pageHtml = Cache.Get<string>("Corona");
                bool cached = true;
                if (pageHtml == null)
                {
                    cached = false;
                    Uri pageUri = new Uri("https://ncov2019.live/data");
                    pageHtml = Http.Get(pageUri, fuzzUserAgent: true, compression: true, timeout: 20000);
                    Cache.Set("Corona", pageHtml, DateTimeOffset.Now.AddMinutes(1));
                }
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageHtml);
                var row = document.DocumentNode.SelectSingleNode($"//td[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'{country}')]");
                if (row == null)
                {
                    return new List<string>{"Can't find the country region, refer to the table at https://ncov2019.live/ to see acceptable names."};
                }
                var countryFromTable = row.InnerText.Replace("&#9733;", string.Empty).Trim();

                var columns = row.ParentNode.ChildNodes.Where(c => c.Name == "td").ToList();

                string cases = columns[1]?.InnerText?.Trim();
                var caseChange = columns[2]?.InnerText?.Trim() + $" ({columns[3]?.InnerText?.Trim()}%)";
                string critical = columns[4]?.InnerText?.Trim();
                string deaths = columns[5]?.InnerText?.Trim();
                var deathChange = columns[6]?.InnerText?.Trim() + $" ({columns[7]?.InnerText?.Trim()}%)";
                string recovered = columns[8]?.InnerText?.Trim();
                string active = columns[9]?.InnerText?.Trim();
                if (cases == null || cases.Trim() == string.Empty)
                {
                    cases = "None/Unknown";
                }
                if (deaths == null || deaths.Trim() == string.Empty)
                {
                    deaths = "None/Unknown";
                }
                if (recovered == null || recovered.Trim() == string.Empty)
                {
                    recovered = "None/Unknown";
                }
                if (caseChange.Trim() == string.Empty)
                {
                    caseChange = "None/Unknown";
                }

                if (deathChange.Trim() == string.Empty)
                {
                    deathChange = "None/Unknown";
                }

                if (active == null || active.Trim() == string.Empty)
                {
                    active = "None/Unknown";
                }
                
                if (critical == null || critical.Trim() == string.Empty)
                {
                    critical = "None/Unknown";
                }
                
                var lastUpdatedNode = document.DocumentNode.SelectSingleNode("//p[contains(text(), \"updated:\")]//i");
                string lastUpdatedText = lastUpdatedNode?.InnerText ?? "Unknown";

                return new List<string>{$"{countryFromTable}: Total Cases: {cases} (+{caseChange}), Active: {active}, Critical: {critical}, Deaths: {deaths} (+{deathChange}), Recovered: {recovered}); Last Updated: {lastUpdatedText}, Cached: {cached}"};
            }
        }
    }
}