using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using WeinerBot.Models;
using WeinerBot.Services;

namespace WeinerBot.Modules.Commands
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
            Dictionary<string, AirportModel> airports = Cache.Get<Dictionary<string, AirportModel>>("Airports");
            if (airports == null)
            {
                airports = Configs.ConfigHelpers.LoadConfig<Dictionary<string, AirportModel>>(Configs.ConfigHelpers
                    .ConfigPaths.AirportConfig);
            }
            Cache.Set("Airports", airports, DateTimeOffset.Now.AddDays(1));
            AirportModel airport;
            try
            {
                airport = airports.Single(a =>
                    string.Equals(a.Value.IATA, airportCode, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(a.Value.ICAO, airportCode, StringComparison.CurrentCultureIgnoreCase)).Value;
            }
            catch
            {
                return new List<string>{"Airport probably doesn't exist"};
            }

            if (airport == null)
            {
                return new List<string>{"Airport probably doesn't exist"};
            }
            Uri metarDataUri = new Uri($"https://www.aviationweather.gov/metar/data?ids={airport.ICAO}&format=raw&hours=0&taf=off&layout=off");
            string pageHtml = Http.Get(metarDataUri, fuzzUserAgent: true, compression: true);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(pageHtml);
            var metarElement = document.DocumentNode.SelectSingleNode("//code");
            if (metarElement == null)
            {
                return new List<string>{"Could not find METAR data for airport"};
            }
            return new List<string>{airport.Name + ": " + metarElement.InnerText};
        }
    }
}