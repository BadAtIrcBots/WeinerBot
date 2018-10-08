using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Configs;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Models.Config;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    public class RcpCommand : ICommand
    {
        public string CommandName { get; } = "Get RCP Poll";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^rcp (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Gets the latest RCP poll data. Argument is an alias for an ID which is defined in rcp_polls.json or the ID itself. You can get the ID from the URL, e.g. for URL https://www.realclearpolitics.com/epolls/other/2018_generic_congressional_vote-6185.html the ID is 6185";
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            RcpConfigModel.RcpConfigRoot config =
                ConfigHelpers.LoadConfig<RcpConfigModel.RcpConfigRoot>(ConfigHelpers.ConfigPaths.RcpConfig);
            string alias = arguments[1].Value;
            int pollId = 0;
            try
            {
                pollId = int.Parse(alias);
            }
            catch (Exception)
            {
                // ignored
            }

            if (pollId == 0)
            {
                var poll = config.RcpPolls.Find(r => string.Equals(r.Name, alias, StringComparison.CurrentCultureIgnoreCase));
                if (poll == null)
                {
                    string availableAliases = string.Empty;
                    foreach (var availablePolls in config.RcpPolls)
                    {
                        availableAliases += availablePolls.Name + ", ";
                    }
                    return new List<string>{"RCP poll alias not defined in rcp_polls.json, available aliases are: " + availableAliases.TrimEnd(' ').TrimEnd(',')};
                }
                pollId = poll.Id;
            }

            RcpModels.RcpPollRoot pollData;
            try
            {
                pollData = JsonConvert.DeserializeObject<RcpModels.RcpPollRoot>(
                    new Regex(@"return_json\((.+)\);")
                        .Match(Http.Get(
                            new Uri($"https://www.realclearpolitics.com/epolls/json/{pollId}_historical.js")))
                        .Groups[1].Value);
            }
            catch (Http.HttpException e)
            {
                if (e.Code == HttpStatusCode.Forbidden)
                {
                    return new List<string>{$"Poll ID {pollId} doesn't seem to exist"};
                }
                return new List<string>{$"Got {e.Code} when requesting {pollId}"};
            }

            var latest = pollData.Poll.RcpAverage.OrderByDescending(x => x.Date).First();
            string response = $"{pollData.Poll.Title}";
            foreach (var candidate in latest.Candidates)
            {
                response += $" - {candidate.Name}: {candidate.Value}";
            }

            response += $" - Date: {latest.Date.ToShortDateString()} - Link: {pollData.Poll.Link}";
            return response.SplitInParts().ToList();
        }
    }
}