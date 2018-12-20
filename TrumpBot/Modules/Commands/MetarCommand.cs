using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    public class MetarCommand : ICommand
    {
        public string CommandName { get; } = "Get METAR data by ICAO code";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^metar (\w+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Gets METAR data from aviationweather.gov, use the ICAO code, not IATA";
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            string airportCode = arguments[1].Value;
            Uri metarDataUri = new Uri($"https://www.aviationweather.gov/metar/data?ids={airportCode}&format=raw&hours=0&taf=off&layout=off");
            string pageHtml = Http.Get(metarDataUri, fuzzUserAgent: true, compression: true);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageHtml);
            var metarElement = document.DocumentNode.SelectSingleNode("//code");
            if (metarElement == null)
            {
                return new List<string>{"Could not find METAR data for airport"};
            }
            return new List<string>{metarElement.InnerText};
        }
    }
}