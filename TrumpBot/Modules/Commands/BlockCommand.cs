using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    [Command.UseMainThread]
    internal class BlockMainThreadCommand : ICommand
    {
        public string CommandName { get; } = "BlockMainThreadCommand";
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
        public string CommandName { get; } = "BlockThreadCommand";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^block thread$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            Thread.Sleep(5000);
            return new List<string>{"💤 Slept for 5 seconds"};
        }
    }
}