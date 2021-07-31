using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Humanizer;
using Newtonsoft.Json;
using WeinerBot.Extensions;
using WeinerBot.Models;
using WeinerBot.Services;

namespace WeinerBot.Modules.Commands
{
    public class UrbanDictionaryCommands
    {
        public class GetUdTerm : ICommand
        {
            public string CommandName { get; } = "Get Urban Dictionary Term";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^urban (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^ud (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Get term from Urban Dictionary";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                string term = arguments[1].Value.ToLower().TrimEnd();
                int index;

                string maybeIndex = term.Split(' ').Last();
                
                if (!int.TryParse(maybeIndex, out index))
                {
                    index = 1;
                }
                else
                {
                    int count = term.Split(' ').Length;
                    var fuckingkillme = term.Split(' ').ToList();
                    fuckingkillme.RemoveAt(count - 1);
                    term = string.Join(" ", fuckingkillme.ToArray());
                }

                if (index == 0)
                {
                    return new List<string>{"Nice try asshole"};
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

                return
                    $"<{item.Author}> Up: {item.ThumbsUp}, Down: {item.ThumbsDown}, \"{item.Definition.ReplaceNewlines(" ")}\", ex: \"{item.Example.ReplaceNewlines()}\", {item.WrittenOn.Humanize(true, DateTime.UtcNow)}, {item.Permalink}"
                        .Truncate(1290).SplitInParts().ToList();
            }
        }
    }
}