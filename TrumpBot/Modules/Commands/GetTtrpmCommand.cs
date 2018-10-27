using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class GetTtrpmCommand : ICommand
    {
        public string CommandName { get; } = "Get Trump Tweet Replies / Minute";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^ttrpm$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^ttrpm (\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Get the average number of Trump tweet replies per minute over a given period of time, by default 5 minutes. Maximum amount of time is 60 minutes since the bot will rollover after that.";
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            int period = 5;
            if (arguments.Count > 1)
            {
                if (int.TryParse(arguments[1].Value, out period))
                {
                    if (period > 60)
                    {
                        return new List<string>{"Period cannot be greater than 60 minutes"};
                    }
                }
            }

            List<int> values = new List<int>();
            List<int> ttrpm = Services.Cache.Get<List<int>>("TTRPM");
            var time = DateTime.Now;
            int i = 1; // Shift back to previous minute so we don't get a partial result
            while (values.Count < period)
            {
                var minute = time.Minute - i;
                if (minute < 0) // Wrap around
                {
                    minute = minute + 60;
                }
                values.Add(ttrpm[minute]);
                i++;
            }
            return new List<string>{$"Average Trump Tweet Replies / min over the last {period} minutes: {values.Average()}"};
        }
    }
}