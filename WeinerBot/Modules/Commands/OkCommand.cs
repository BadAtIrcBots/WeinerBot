using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WeinerBot.Models;

namespace WeinerBot.Modules.Commands
{
    [Command.NoPrefix]
    public class OkCommand : ICommand
    {
        public string CommandName { get; } = "👌 Command";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("👌", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Returns a 👌 for every usage of 👌";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            int count = new Regex(Regex.Escape("👌")).Matches(messageEvent.Message).Count;
            return new List<string>{ String.Concat(Enumerable.Repeat("👌", count))};
        }
    }
}
