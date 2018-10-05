using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Extensions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class ReeCommand : ICommand
    {
        public string CommandName { get; } = "Ree!";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex> {new Regex(@"^ree", RegexOptions.Compiled | RegexOptions.IgnoreCase)};
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "REEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            List<int> randomRange = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText("Config\\ree.json"));
            if (randomRange.Count <= 1)
            {
                randomRange = new List<int>{50,450};
            }
            
            int reeSize = new Random(Guid.NewGuid().GetHashCode()).Next(randomRange[0], randomRange[1]);
            if (messageEvent.Message.StartsWith("REE"))
            {
                reeSize = reeSize * 2;
            }
            return ("R" + new string('E', reeSize)).SplitInParts(430).ToList();
        }
    }
}
