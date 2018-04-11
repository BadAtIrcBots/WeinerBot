using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Extensions;

namespace TrumpBot.Modules.Commands
{
    public class ReeCommand : ICommand
    {
        public string CommandName { get; } = "Ree!";
        public List<Regex> Patterns { get; set; } = new List<Regex> {new Regex(@"^ree$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};
        public List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true)
        {
            List<int> randomRange = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText("Config\\ree.json"));
            if (randomRange.Count <= 1)
            {
                randomRange = new List<int>{50,450};
            }

            int reeSize = new Random(Guid.NewGuid().GetHashCode()).Next(randomRange[0], randomRange[1]);
            if (message == "REE")
            {
                reeSize = reeSize * 2;
            }
            return ("R" + new string('E', reeSize)).SplitInParts(430).ToList();
        }
    }
}
