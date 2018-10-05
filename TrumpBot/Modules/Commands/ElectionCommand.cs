using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class ElectionCommand : ICommand
    {
        public string CommandName { get; } = "Get 2016 election results";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^election (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^demelection (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^results (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^demresults (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Get the 2016 election results, provide state as an argument or 'us' to get national results.";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            string result = "";

            if (arguments == null || arguments.Count == 1) throw new Exception("An argument is required");

            string state = arguments[1].Value.ToLower();

            if (state == "list" || state == "help")
            {
                result += "Use two letter state code or full state name: e.g. Alabama or AL; Names are case insensitive";
                return new List<string>{result};
            }

            bool isDemocrat = messageEvent.Message.StartsWith("demelection") || messageEvent.Message.StartsWith("demresults");
            Calendar electionCalendar = new ElectionModel().GetCalendar(useCache);

            Election currentElection;

            if (!isDemocrat)
            {
                currentElection = (from race in electionCalendar.Races
                    where
                        (race.State.ToLower() == state || race.RaceId.ToLower().StartsWith(state))
                    select race).FirstOrDefault();
            }
            else
            {
                currentElection = (from race in electionCalendar.Races
                    where
                        (race.State.ToLower() == state || race.RaceId.ToLower().StartsWith(state))
                    select race).FirstOrDefault();
            }
            

            if (currentElection == null) return new List<string>{"Looks like this state doesn't exist. (Try the full name, e.g. Alabama or state code, e.g. AL)"};

            DateTime updateDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(currentElection.Timestamp);
            DateTime closeDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(currentElection.PollCloseEpoch);

            result = $"{currentElection.State} Results:";
            result = currentElection.Candidates.Aggregate(result,
                (current, candidate) => current + $" {candidate.LastName}: {candidate.Votes:n0} votes ({candidate.Percent}%) |");
            result +=
                $" Last updated: {(int) (DateTime.UtcNow - updateDate).TotalMinutes} minutes ago - Polls close in {(int) (closeDate - TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))).TotalHours} hours - {currentElection.PercentageReporting}% reporting";

            return new List<string>{result};
        }
    }
}
