using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WeinerBot.Extensions;
using WeinerBot.Models;

namespace WeinerBot.Modules.Commands
{
    public class DiceCommand : ICommand
    {
        public string CommandName { get; } = "Dice";

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^roll (\d+)d(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^roll (\d+)d(\d+) (\W)(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = false;

        public string HelpDescription { get; set; } =
            "Roll a dice using D&D-style syntax, e.g. roll 12d2 or with a modifier, 12d2 +3 or -3";
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());

            int rolls = int.Parse(arguments[1].Value);
            if (rolls > 100)
            {
                return new List<string> {"Too many rolls!"};
            }
            int dieSize = int.Parse(arguments[2].Value);
            bool modifier = false;
            bool subtract = false;
            bool add = false;
            int modifierValue = 0;

            if (arguments.Count > 3)
            {
                modifier = true;
                if (arguments[3].Value == "+") add = true;
                if (arguments[3].Value == "-") subtract = true;
                if (!add && !subtract)
                {
                    return new List<string> {"Please provide a valid operator of either + or -"};
                }
                modifierValue = int.Parse(arguments[4].Value);
            }

            if(dieSize <= 0) return new List<string> {"Die size needs to be greater than 0"};

            if(rolls <= 0) return new List<string> {"There needs to be more than 0 rolls"};

            List<int> rollResults = new List<int>(rolls);

            int i = 0;
            while (i < rolls)
            {
                var currentResult = random.Next(1, dieSize + 1);
                if (modifier)
                {
                    if (add) currentResult += modifierValue;
                    if (subtract) currentResult -= modifierValue;
                }

                rollResults.Add(currentResult);

                i++;
            }

            string result = $"{messageEvent.Nick}'s roll results:";

            result = rollResults.Aggregate(result, (current, roll) => current + $" {roll},");

            result += $" Rolled {rolls}d{dieSize}";
            if (!modifier) return result.SplitInParts().ToList();
            
            if (add)
            {
                result += $" with a +{modifierValue} modifier";
            }
            if (subtract)
            {
                result += $" with a -{modifierValue} modifier";
            }

            return result.SplitInParts().ToList();
        }
    }
}