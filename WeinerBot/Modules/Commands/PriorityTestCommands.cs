using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WeinerBot.Models;

namespace WeinerBot.Modules.Commands
{
    public class PriorityTestCommands
    {
        internal class VeryHighPriorityCommand : ICommand
        {
            public string CommandName { get; } = "Very High Priority Test";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^prioritytest$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = true;
            public string HelpDescription { get; set; } = "When you run !the prioritytest command, this should appear first";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.VeryHigh;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"This is the high priority command, this should be first. Ticks: {DateTime.UtcNow.Ticks}"};
            }
        }

        internal class NormalPriorityCommand : ICommand
        {
            public string CommandName { get; } = "Normal Priority Test";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^prioritytest$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = true;
            public string HelpDescription { get; set; } = "When you run !the prioritytest command, this should appear second";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"This is the normal priority command, this should be second. Ticks: {DateTime.UtcNow.Ticks}"};
            }
        }

        internal class LowPriorityCommand : ICommand
        {
            public string CommandName { get; } = "Low Priority Test";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^prioritytest$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public bool HideFromHelp { get; set; } = true;
            public string HelpDescription { get; set; } = "When you run !the prioritytest command, this should appear third";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Low;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new List<string>{$"This is the low priority command, this should be third. Ticks: {DateTime.UtcNow.Ticks}"};
            }
        }
    }
}