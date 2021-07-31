using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WeinerBot.Models;

namespace WeinerBot.Modules.Commands
{
    public class CurrentYearCommands
    {
        [Command.NoPrefix]
        [Command.CacheOutput(60)]
        internal class CurrentYear : ICommand
        {
            public string CommandName { get; } = "Current Year";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"current year", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Lets you know what the current year is.";

            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"Oh my gosh it is {DateTime.UtcNow.Year}"};
            }
        }

        [Command.CacheOutput(60)]
        internal class GetCurrentYear : ICommand
        {
            public string CommandName { get; } = "Get Current Year";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^year$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetch the current year.";

            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"It's {DateTime.UtcNow.Year}!"};
            }
        }

        internal class GetCurrentDateTime : ICommand
        {
            public string CommandName { get; } = "Get Current Date and Time";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^date$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^time$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetch the current server clock.";

            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"It is {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} UTC according to the server clock."};
            }
        }
    }
}