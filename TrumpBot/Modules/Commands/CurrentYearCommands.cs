using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TrumpBot.Modules.Commands
{
    public class CurrentYearCommands
    {
        [Command.NoPrefix]
        [Command.CacheOutput(60)]
        internal class CurrentYear : ICommand
        {
            public string CommandName { get; } = "CurrentYear";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"current year", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            
            public List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"Oh my gosh it is {DateTime.UtcNow.Year}"};
            }
        }

        [Command.CacheOutput(60)]
        internal class GetCurrentYear : ICommand
        {
            public string CommandName { get; } = "GetCurrentYear";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^year$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"It's {DateTime.UtcNow.Year}!"};
            }
        }

        internal class GetCurrentDateTime : ICommand
        {
            public string CommandName { get; } = "GetCurrentDateTime";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^date$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^time$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"It is {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} UTC according to the server clock."};
            }
        }
    }
}