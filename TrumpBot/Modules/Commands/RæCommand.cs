using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Extensions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class RæCommand : ICommand
    {
        public string CommandName { get; } = "Ræ";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "rææææææææææææææææææææææææææææææææææææææææææææææææææææææææ";

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^ræ$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^rae$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            return ("R" + String.Concat(Enumerable.Repeat("Æ", new Random().Next(50, 450)))).SplitInParts(430).ToList();
        }
    }
}
