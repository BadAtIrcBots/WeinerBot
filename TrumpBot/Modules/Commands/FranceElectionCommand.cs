using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Extensions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(600)]
    public class FranceElectionCommand : ICommand
    {
        public string CommandName { get; } = "Get France Election Results";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^fr$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; }

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            List<FranceElectionApiModel.Election> electionData = new FranceElectionApiModel().GetElectionData(useCache);

            FranceElectionApiModel.Election currentElectionData = electionData.Last();

            string result = $"France {currentElectionData.Year} round {currentElectionData.Round} results:";

            result = currentElectionData.Votes.Aggregate(result,
                (current, candidate) =>
                    current + $" {candidate.Name}: {candidate.Votes:n0} votes ({candidate.Percent}%);");

            DateTime updateDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(currentElectionData.UpdatedAt);

            result += $" Last Updated: {(int) (DateTime.UtcNow - updateDate).TotalMinutes} minutes ago";

            return result.SplitInParts(430).ToList();
        }
    }
}
