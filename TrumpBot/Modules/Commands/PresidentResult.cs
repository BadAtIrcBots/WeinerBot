using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    class PresidentResult : ICommand
    {
        public string CommandName { get; } = "Get head on head results";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^president (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            string result = "";

            List<OneVsOneModel.OneVsOne> polls = JsonConvert.DeserializeObject<List<OneVsOneModel.OneVsOne>>(File.ReadAllText("Config\\presidents.json"));

            if (arguments == null || arguments.Count == 1) throw new Exception("An argument is required");
            string scenario = arguments[1].Value.ToLower();

            if (scenario == "list" || scenario == "help")
            {
                result += "Supported scenarios: ";
                result = polls.Aggregate(result, (current, poll) => current + $"{poll.Name}; ");
                result += "Names are case-insensitive";
                return new List<string> {result};
            }

            OneVsOneModel.OneVsOne currentChart = null;
            foreach (OneVsOneModel.OneVsOne poll in polls)
            {
                string[] splitStrings = arguments[1].Value.ToLower().Split(' ');
                if (splitStrings.Length == 3)
                {
                    splitStrings[1] = splitStrings[2]; // the bodge is strong with this one
                }
                if ((poll.FirstCandidate.Contains(splitStrings[0]) && poll.SecondCandidate.Contains(splitStrings[1])) || (poll.SecondCandidate.Contains(splitStrings[0]) && poll.FirstCandidate.Contains(splitStrings[1])))
                {
                    currentChart = poll;
                    break;
                }

            }

            if (currentChart == null) return new List<string> {"Scenario given isn't in my list"};

            List<Chart> charts = new PollChart().GetCharts("2016-president", useCache);
            bool matched = false;

            foreach (Chart chart in charts.Where(chart => chart.Slug == currentChart.Slug))
            {
                result += $"{chart.ShortTitle}:";
                result = chart.Estimates.Aggregate(result,
                    (current, candidateEstimate) =>
                        current + $" {candidateEstimate.Choice}: {candidateEstimate.Value}% |");
                result += $" Last Updated: {chart.LastUpdated.ToString("yyyy-MM-dd")} - {currentChart.ShortUri.AbsoluteUri}";
                if (chart.ElectionDate.HasValue)
                {
                    result += $" - Election Date: {chart.ElectionDate.Value.ToString("yyyy-MM-dd")}";
                }
                matched = true;
                break;
            }

            if (!matched) throw new Exception($"For some reason {currentChart.Slug} was missing from the result.");

            return new List<string> {result};
        }
    }
}
