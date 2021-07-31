using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WeinerBot.Models;

namespace WeinerBot.Modules.Commands
{
    internal class CacheTestCommands
    {
        [Command.CacheOutput(600)]
        internal class TestCacheIsWorkingCommand : ICommand
        {
            public string CommandName { get; } = "Test Cache Is Working";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^cachetest$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Returns the time when the method was last accessed and caches this for 600 seconds so you can test if the cache is actually working";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                return new() { $"Time as of execution {DateTime.UtcNow}" };
            }
        }
    }
}