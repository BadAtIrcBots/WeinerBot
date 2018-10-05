using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Models.Config;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    [Command.NoPrefix]
    [Command.DoNotReportException]
    internal class UrlMention : ICommand
    {
        public string CommandName { get; } = "URL Mention Handler";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"(https?)://(-\.)?([^\s/?\.#]+\.?)+(/[^\s]*)?", RegexOptions.Compiled | RegexOptions.Multiline)
        };
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.High;
        public bool HideFromHelp { get; set; } = true;
        public string HelpDescription { get; set; } = "Matches URLs and stores them in a database so that users can later be berated for reusing links.";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            Uri matchedUri = new Uri(arguments[0].Value);
            var mentionConfig = UrlHistory.GetConfig();
            var mention =
                UrlHistory.GetUrlMention(mentionConfig, messageEvent.Channel, matchedUri.AbsoluteUri);
            if (mention == null)
            {
                UrlHistory.AddUrlMention(messageEvent.Nick, messageEvent.Channel, matchedUri.AbsoluteUri);
                return null;
            }

            if (!UrlHistory.IsChannelEnabled(messageEvent.Channel))
            {
                return null;
            }

            if (String.Equals(mention.User, messageEvent.Nick, UrlHistory.ComparisonCulture))
            {
                return null;
            }

            return UrlHistory.FormatResponse(mention, messageEvent.Nick,
                UrlHistory.GetResponse(mentionConfig, messageEvent.Channel),
                preventHighlight: UrlHistory.ShouldPreventNickHighlight(mention.User)).SplitInParts(430).ToList();
        }
    }
}