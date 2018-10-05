using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(600)]
    public class DelegateCommand : ICommand
    {
        public string CommandName { get; } = "Get delegates";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("@^delegates$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Fetches the 2016 GOP Primary delegate stats.";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            DelegateModel.DelegateStatistics delegateStatistics =
                new DelegateModel().GetDelegateStatistics(
                    new Uri("http://data.cnn.com/ELECTION/2016primary/full/R.json"), useCache);

            Dictionary<int, DelegateModel.Candidate> candidates = delegateStatistics.Candidates.ToDictionary(candidate => candidate.Id);

            delegateStatistics.NationalRaces.Sort((x, y) => y.TotalDelegates.CompareTo(x.TotalDelegates));

            string result = "Delegates:";
            int i = 0;
            foreach (DelegateModel.NationalRace race in delegateStatistics.NationalRaces.TakeWhile(race => i != 5))
            {
                result += $" {candidates[race.Id].LastName}: {race.TotalDelegates} ({Math.Round((race.TotalDelegates / (decimal)delegateStatistics.RequiredDelegates) * 100, 2)}%) |";
                i++;
            }

            DateTime updateTime = new DateTime(1970, 1, 1, 0, 0 ,0, DateTimeKind.Utc).AddMilliseconds(delegateStatistics.Timestamp);

            result += $" Last updated: {(int) (DateTime.UtcNow - updateTime).TotalHours} hours ago";

            return new List<string>{result};
        }
    }
}
