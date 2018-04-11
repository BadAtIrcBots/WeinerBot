using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    internal class DebateCommand : ICommand
    {
        public string CommandName { get; } = "Get next debate";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^debate$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true)
        {
            List<DebateModel.Debate> oldDebateList = JsonConvert.DeserializeObject<List<DebateModel.Debate>>(File.ReadAllText("Config\\debates.json"));

            List<DebateModel.Debate> debates = oldDebateList.Where(debate => debate.Date.UtcTicks - DateTimeOffset.UtcNow.UtcTicks > 0).ToList();

            debates.Sort((x, y) => x.Date.CompareTo(y.Date));
            DebateModel.Debate currentDebate = debates[0];

            TimeSpan difference = currentDebate.Date.UtcDateTime - DateTimeOffset.UtcNow;

            string result = $"The next debate is happening in {difference.Days} days";
            if (currentDebate.TimeKnown)
            {
                result += $", {difference.Hours} hours";
            }

            return new List<string>{ result + $" ({currentDebate.Date:yyyy-MM-dd}). It will be held at {currentDebate.Location} and hosted by {currentDebate.Host}" };
        }
    }
}
