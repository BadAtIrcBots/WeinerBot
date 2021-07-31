using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using WeinerBot.Models;

namespace WeinerBot.Modules.Commands
{
    [Command.UseMainThread]
    internal class BlockMainThreadCommand : ICommand
    {
        public string CommandName { get; } = "Block main thread";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = true;
        public string HelpDescription { get; set; } = "Blocks the main thread for 5 seconds";

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^block main$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            Thread.Sleep(5000);
            return new List<string>{"💤 Slept for 5 seconds"};
        }
    }
    
    internal class BlockThreadCommand : ICommand
    {
        public string CommandName { get; } = "Block command thread";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^block thread$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public bool HideFromHelp { get; set; } = true;
        public string HelpDescription { get; set; } = "Blocks its own command thread for 5 seconds";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            Thread.Sleep(5000);
            return new List<string>{"💤 Slept for 5 seconds"};
        }
    }
}