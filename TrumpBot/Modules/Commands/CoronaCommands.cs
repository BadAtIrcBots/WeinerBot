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
                string pageHtml = Cache.Get<string>("Worldometers");
                bool cached = true;
                if (pageHtml == null)
                {
                    cached = false;
                    Uri pageUri = new Uri("https://www.worldometers.info/coronavirus/");
                    pageHtml = Http.Get(pageUri, fuzzUserAgent: true, compression: true, timeout: 20000);
                    Cache.Set("Worldometers", pageHtml, DateTimeOffset.Now.AddMinutes(1));
                }
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageHtml);
                var nodes = document.DocumentNode.SelectNodes("//div[@class=\"maincounter-number\"]");
                if (nodes.Count < 3)
                {
                    return new List<string>{"Page scraping is broken (nodes.Count < 3 for maincounter-number)"};
                }

                string cases = nodes[0]?.ChildNodes.First(c => c.Name == "span")?.InnerText?.Trim();
                string deaths = nodes[1]?.ChildNodes.First(c => c.Name == "span")?.InnerText?.Trim();
                string recovered = nodes[2]?.ChildNodes.First(c => c.Name == "span")?.InnerText?.Trim();

                if (cases == null)
                {
                    return new List<string>{"Parsing of total case numbers is broken"};
                }

                if (deaths == null)
                {
                    return new List<string>{"Parsing of total deaths is broken"};
                }

                if (recovered == null)
                {
                    return new List<string>{"Parsing of recovered totals is broken"};
                }

                var lastUpdatedNode = document.DocumentNode.SelectSingleNode("//div[contains(text(), \"Last updated\")]");
                var lastUpdatedSuccess = DateTime.TryParseExact(lastUpdatedNode.InnerText.Replace("Last updated: ", ""),
                    "MMMM dd, yyyy, HH:mm GMT", null, DateTimeStyles.AssumeLocal, out var lastUpdated);
                if (!lastUpdatedSuccess)
                {
                    return new List<string>{$"Cases: {cases}; Deaths: {deaths}; Recovered: {recovered}"};
                }

                var relativeTime = lastUpdated.Humanize(dateToCompareAgainst: DateTime.Now, utcDate: false);
                return new List<string>{$"Cases: {cases}; Deaths: {deaths}; Recovered: {recovered}; Last Updated: {relativeTime}, Cached: {cached}"};
            }
        }

        internal class GetCoronaCountryStats : ICommand
        {
            public string CommandName { get; } = "Get Coronavirus Country Stats";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^coron\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^viru\S+ (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Get Coronavirus stats by country";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                string country = arguments[1].Value.ToLower();
                string pageHtml = Cache.Get<string>("Worldometers");
                bool cached = true;
                if (pageHtml == null)
                {
                    cached = false;
                    Uri pageUri = new Uri("https://www.worldometers.info/coronavirus/");
                    pageHtml = Http.Get(pageUri, fuzzUserAgent: true, compression: true, timeout: 20000);
                    Cache.Set("Worldometers", pageHtml, DateTimeOffset.Now.AddMinutes(1));
                }
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageHtml);
                HtmlNode countryRow;
                string countryFromTable;
                // For rows with clickable links
                var rowHref = document.DocumentNode.SelectSingleNode($"//a[translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{country}']");
                if (rowHref == null)
                {
                    // For some reason there are spaces around the country
                    var rowTd = document.DocumentNode.SelectSingleNode($"//td[translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{country}']");
                    if (rowTd == null)
                    {
                        return new List<string>{"Can't find country in the table"};
                    }

                    countryFromTable = rowTd.InnerText.Trim();
                    countryRow = rowTd.ParentNode;
                }
                else
                {
                    countryFromTable = rowHref.InnerText.Trim();
                    countryRow = rowHref.ParentNode.ParentNode;
                }

                var columns = countryRow.ChildNodes.Where(c => c.Name == "td").ToList();
                
                string cases = columns[1]?.InnerText?.Trim();
                string deaths = columns[3]?.InnerText?.Trim();
                string recovered = columns[5]?.InnerText?.Trim();
                string active = columns[6]?.InnerText?.Trim();
                string serious = columns[7]?.InnerText?.Trim();
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
                if (active == null || active.Trim() == string.Empty)
                {
                    active = "None/Unknown";
                }
                if (serious == null || serious.Trim() == string.Empty)
                {
                    serious = "None/Unknown";
                }
                
                var lastUpdatedNode = document.DocumentNode.SelectSingleNode("//div[contains(text(), \"Last updated\")]");
                var lastUpdatedSuccess = DateTime.TryParseExact(lastUpdatedNode.InnerText.Replace("Last updated: ", ""),
                    "MMMM dd, yyyy, HH:mm GMT", null, DateTimeStyles.AssumeLocal, out var lastUpdated);
                if (!lastUpdatedSuccess)
                {
                    return new List<string>{$"Cases: {cases}; Deaths: {deaths}; Recovered: {recovered}"};
                }
                var relativeTime = lastUpdated.Humanize(dateToCompareAgainst: DateTime.Now, utcDate: false);
                
                return new List<string>{$"{countryFromTable}: Cases: {cases} (Active: {active}, Serious: {serious}, Deaths: {deaths}, Recovered: {recovered}); Last Updated: {relativeTime}, Cached: {cached}"};
            }
        }
    }
}