using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.Commands
{
    internal class ElectionCountdownCommand : ICommand // Wow this won't get dated at all! 9:00 PM 04/10/2016
    {
        public string CommandName { get; } = "ElectionCountdown";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^countdown$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^election$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            ElectionDateTimeConfigModel electionDateTime =
                ConfigHelpers.LoadConfig<ElectionDateTimeConfigModel>(ConfigHelpers.ConfigPaths.ElectionDateTimeConfig);
            if (electionDateTime == null)
            {
                throw new Exception("electionDateTime was null");
            }
            TimeSpan difference = electionDateTime.ElectionDateTime.ToUniversalTime() - DateTime.UtcNow;
            return new List<string>
            {
                $"{(int) difference.TotalDays} days, {difference.Hours} hours and {difference.Minutes} minutes until the United States 2016 Presidential Election scheduled on {electionDateTime.ElectionDateTime.Date.ToLongDateString()}"
            };
        }
    }
}
