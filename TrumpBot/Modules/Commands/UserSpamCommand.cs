using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class UserSpamCommand
    {
        public class SpamCannapede : ICommand
        {
            public string CommandName { get; } = "Spam Cannapede";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^cannapede$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Make Cannapede mad";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {            
                int count = new Random(Guid.NewGuid().GetHashCode()).Next(10, 40);
                return new List<string>
                {
                    string.Concat(Enumerable.Repeat("Cannapede ", count))
                };
            }
        }
    }
}