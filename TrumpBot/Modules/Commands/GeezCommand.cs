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
    public class GeezCommand : ICommand
    {
        public string CommandName { get; } = "Geez!";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex> {new Regex(@"^geez", RegexOptions.Compiled | RegexOptions.IgnoreCase)};
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "GEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEZ";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            List<int> randomRange;
            try
            {
                randomRange = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText(Path.Join("Config", "geez.json")));
            }
            catch (FileNotFoundException)
            {
                randomRange = new List<int>{50,450};
            }
            if (randomRange.Count <= 1)
            {
                randomRange = new List<int>{50,450};
            }
            
            int geezSize = new Random(Guid.NewGuid().GetHashCode()).Next(randomRange[0], randomRange[1]);
            if (messageEvent.Message.StartsWith("GEEZ"))
            {
                geezSize = geezSize * 2;
            }
            return ("G" + new string('E', geezSize) + "Z").SplitInParts(430).ToList();
        }
    }
}