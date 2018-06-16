using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class PriorityTestCommands
    {
        internal class VeryHighPriorityCommand : ICommand
        {
            public string CommandName { get; } = "VeryHighPriorityCommand";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^prioritytest$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.VeryHigh;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"This is the high priority command, this should be first. Ticks: {DateTime.UtcNow.Ticks}"};
            }
        }

        internal class NormalPriorityCommand : ICommand
        {
            public string CommandName { get; } = "NormalPriorityCommand";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^prioritytest$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"This is the normal priority command, this should be second. Ticks: {DateTime.UtcNow.Ticks}"};
            }
        }

        internal class LowPriorityCommand : ICommand
        {
            public string CommandName { get; } = "LowPriorityCommand";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^prioritytest$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Low;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"This is the low priority command, this should be third. Ticks: {DateTime.UtcNow.Ticks}"};
            }
        }
    }
}