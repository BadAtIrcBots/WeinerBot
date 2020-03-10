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
                new Regex(@"^corona\S+$")
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetch worldwide Coronavirus stats";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                Uri pageUri = new Uri("https://www.worldometers.info/coronavirus/");
                string pageHtml = Http.Get(pageUri, fuzzUserAgent: true, compression: true);
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
                DateTime lastUpdated = DateTime.MinValue;
                var lastUpdatedSuccess = DateTime.TryParseExact(lastUpdatedNode.InnerText.Replace("Last updated: ", ""),
                    "MMMM dd, yyyy, HH:mm GMT", null, DateTimeStyles.AssumeLocal, out lastUpdated);
                if (!lastUpdatedSuccess)
                {
                    return new List<string>{$"Cases: {cases}; Deaths: {deaths}; Recovered: {recovered}"};
                }

                var relativeTime = lastUpdated.Humanize(dateToCompareAgainst: DateTime.Now, utcDate: false);
                return new List<string>{$"Cases: {cases}; Deaths: {deaths}; Recovered: {recovered}; Last Updated: {relativeTime}"};
            }
        }
    }
}