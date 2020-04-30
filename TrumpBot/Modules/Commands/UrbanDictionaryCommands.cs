using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Humanizer;
using Newtonsoft.Json;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    public class UrbanDictionaryCommands
    {
        public class GetUdTerm : ICommand
        {
            public string CommandName { get; } = "Get Urban Dictionary Term";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {   // Abusing that it'll check the regexes in order ;)
                new Regex(@"^urban (.+) (\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^ud (.+) (\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^urban (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^ud (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Get term from Urban Dictionary";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                string term = arguments[1].Value.ToLower().TrimEnd();
                int index = 1;
                if (arguments.Count == 3)
                {
                    index = int.Parse(arguments[2].Value);
                }

                var definitions =
                    JsonConvert.DeserializeObject<UrbanDictionaryApiModel.UrbanDictionaryApiItemListModel>(
                        Http.GetJson(
                            new Uri($"http://api.urbandictionary.com/v0/define?term={WebUtility.HtmlEncode(term)}"),
                            true));

                if (definitions.List.Count == 0)
                {
                    return new List<string>{"No results from Urban Dictionary"};
                }

                if (definitions.List.Count < index)
                {
                    return new List<string>{$"Urban Dictionary only returned {definitions.List.Count} results"};
                }

                var item = definitions.List[index - 1];
                
                return new List<string>
                {
                    $"<{item.Author}> Up: {item.ThumbsUp}, Down: {item.ThumbsDown}, \"{item.Definition.ReplaceNewlines(" ")}\", " +
                    $"ex: \"{item.Example.ReplaceNewlines()}\", " +
                    $"{item.WrittenOn.Humanize(true, DateTime.UtcNow)}, {item.Permalink}"
                };
            }
        }
    }
}