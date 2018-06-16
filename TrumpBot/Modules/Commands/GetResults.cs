using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    internal class GetResults : ICommand
    {
        public string CommandName { get; } = "Get Poll Results";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^polls (\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^polls$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^dempolls$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^dempolls (\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            bool democrat = messageEvent.Message.StartsWith("dempolls");

            List<PollModel.Poll> polls = messageEvent.Message.StartsWith("dempolls") ? new List<PollModel.Poll> { new PollModel.Poll { FriendlyNames = new List<string> { "national", "all" }, Slug = "2016-national-democratic-primary", Name = "National" } } : JsonConvert.DeserializeObject<List<PollModel.Poll>>(File.ReadAllText("Config\\polls.json"));


            string state;

            if (arguments == null || arguments.Count == 1)
            {
                state = "national";
            }
            else
            {
                state = arguments[1].Value.ToLower();
            }

            string result = "";

            if (state == "list" || state == "help")
            {
                result += "Supported regions: ";
                result = polls.Aggregate(result, (current, poll) => current + $"{poll.Name}; ");

                result += "Names are case-insensitive and have no spaces";

                return new List<string>{result};
            }

            List<Chart> charts;

            charts = new PollChart().GetCharts(democrat ? "2016-president-dem-primary" : "2016-president-gop-primary", useCache);

            PollModel.Poll chartName;

            if (state == "latest")
            {
                charts.Sort((x, y) => x.LastUpdated.CompareTo(y.LastUpdated));
                Chart chart = charts[charts.Count - 1];
                chartName = (from poll in polls where poll.FriendlyNames.Contains(chart.State.ToLower()) select poll).FirstOrDefault();
                result += $"{chart.State} Polling:";
                result = chart.Estimates.Aggregate(result, (current, candidateEstimate) => current + $" {candidateEstimate.Choice}: {candidateEstimate.Value}% |");
                if ((int)(DateTime.UtcNow - chart.LastUpdated).TotalHours > 72)
                {
                    result += $" Last Updated: {chart.LastUpdated.ToString("yyyy-MM-dd")}";
                }
                else
                {
                    result += $" Last Updated: {(int)(DateTime.UtcNow - chart.LastUpdated).TotalHours} hours ago";
                }

                if (chartName != null)
                {
                    result += $" - {chartName.ShortUri.AbsoluteUri}";
                }
                else
                {
                    result += $" - {chart.PollUri.AbsoluteUri}";
                }
                if (chart.ElectionDate.HasValue)
                {
                    result += $" - Primary/Caucus Date: {chart.ElectionDate.Value.ToString("yyyy-MM-dd")}";
                }
                return new List<string>{result};
            }

            chartName = (from poll in polls where poll.FriendlyNames.Contains(state) select poll).FirstOrDefault();

            //if(chartName == null) return "State given isn't in my database.";

            if (chartName == null)
            {
                Chart chart = (from someChart in charts where someChart.State.ToLower() == state select someChart).FirstOrDefault();
                if (chart == null)
                {
                    result = "State given isn't in database or doesn't have polling results.";
                    if (state.Length >= 3)
                    {
                        result += " (Tried 2 letter state code?)";
                    }

                    return new List<string>{result};
                }

                if (chart.Estimates.Count == 0)
                {
                    return new List<string>
                    {
                        $"Pollster has no data for {chart.State}, try looking on their website instead: {chart.PollUri}"
                    };
                }

                chartName = new PollModel.Poll { Slug = chart.Slug };
            }


            bool matched = false;

            foreach (Chart chart in charts.Where(chart => chart.Slug == chartName.Slug))
            {
                result = $"{chart.State} Polling:";
                result = chart.Estimates.Aggregate(result, (current, candidateEstimate) => current + $" {candidateEstimate.Choice}: {candidateEstimate.Value}% |");
                if ((int)(DateTime.UtcNow - chart.LastUpdated).TotalHours > 72)
                {
                    result += $" Last Updated: {chart.LastUpdated.ToString("yyyy-MM-dd")}";
                }
                else
                {
                    result += $" Last Updated: {(int)(DateTime.UtcNow - chart.LastUpdated).TotalHours} hours ago";
                }

                if (chartName.ShortUri != null)
                {
                    result += $" - {chartName.ShortUri.AbsoluteUri}";
                }
                else
                {
                    result += $" - {chart.PollUri.AbsoluteUri}";
                }

                if (chart.ElectionDate.HasValue)
                {
                    result += $" - Primary/Caucus Date: {chart.ElectionDate.Value.ToString("yyyy-MM-dd")}";
                }
                matched = true;
                break;
            }

            if (!matched) throw new Exception($"For some reason {chartName.Slug} was missing from the result.");

            return new List<string>{result};
        }
    }
}
