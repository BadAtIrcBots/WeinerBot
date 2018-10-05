using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Humanizer;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Services;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Models;
using HttpMethod = System.Net.Http.HttpMethod;

namespace TrumpBot.Modules.Commands
{
    public class TwitterCommand
    {
        [Command.NoPrefix]
        [Command.UseMainThread]
        [Command.BreakAfterExecution]
        internal class GetTweetByUri : ICommand
        {
            public string CommandName { get; } = "Get Tweet By URI";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Looks for tweet IDs in Twitter URLs and fetches the Tweet data. The response for this command is customized to leave out the Tweet URL as it is just unnecessary duplication.";

            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"https?:\/\/twitter\.com\/(?:\#!\/)?(\w+)\/(?:status|statuses)\/(\d+)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase)
            };
            
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                List<string> enabledChannels = JsonConvert
                    .DeserializeObject<ChannelUriConfig>(File.ReadAllText("Config\\uri.json")).TwitterEnabledChannels;

                if (!enabledChannels.Contains(messageEvent.Channel)) return null;

                string twitterHandle = arguments[1].Value;
                long tweetId = long.Parse(arguments[2].Value);

                ITweet tweet = Tweet.GetTweet(tweetId);
                return FormatTweet(tweet, false).SplitInParts(430).ToList();
            }
        }

        [Command.NoPrefix]
        [Command.UseMainThread]
        [Command.BreakAfterExecution]
        internal class GetTweetByShortUri : ICommand
        {
            public string CommandName { get; } = "Get Tweet By Short URI";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetches t.co links and either fetches the Tweet referenced or reveals where the URL redirect is going if it is off site.";

            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"https?:\/\/t\.co\/(\w+)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase)
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                Uri shortUri = new Uri(arguments[0].Value);
                Uri fullUri;

                using (HttpClient client = new HttpClient(new HttpClientHandler{AllowAutoRedirect = false}))
                {
                    HttpRequestMessage headRequest = new HttpRequestMessage(HttpMethod.Head, shortUri);
                    headRequest.Headers.UserAgent.ParseAdd(
                        "curl/7.51.0");
                    headRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    headRequest.Headers.Host = "t.co";

                    HttpResponseMessage headResponse = client.SendAsync(headRequest).Result;

                    fullUri = headResponse.Headers.Location;
                }

                if (!fullUri.Host.Contains("twitter.com"))
                {
                    return $"URL: {fullUri.AbsoluteUri}".SplitInParts(430).ToList();
                }

                GroupCollection groups = new Regex(@"https?:\/\/twitter\.com\/(?:\#!\/)?(\w+)\/(?:status|statuses)\/(\d+)")
                    .Match(fullUri.AbsoluteUri).Groups;

                long tweetId = long.Parse(groups[2].Value);

                ITweet tweet = Tweet.GetTweet(tweetId);
                return FormatTweet(tweet).SplitInParts(430).ToList();
            }
        }

        internal class GetTweetByScreenName : ICommand
        {
            public string CommandName { get; } = "Get Tweet By Screen Name";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetches a tweet by the screen name, with or without the @. You can provide an index as the second argument to navigate the timeline up to 200 tweets back.";

            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^tw (\S+)$"),
                new Regex(@"^tw (\S+) (\d+)$")
            };
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                var screenName = arguments[1].Value;
                int index = 1;

                if (arguments.Count == 3)
                {
                    index = int.Parse(arguments[2].Value);
                }

                if (index < 1)
                {
                    return "Try a number larger than 0".SplitInParts(430).ToList();
                }
                if (index > 200)
                {
                    return "Twitter limitations do not allow me to retrieve more than 200 tweets in a user timeline, sorry."
                        .SplitInParts(430).ToList();
                }

                if (screenName[0] == '@')
                {
                    screenName = screenName.TrimStart('@');
                }

                var tweets = Timeline.GetUserTimeline(screenName, maximumTweets: index); // Default is 40 tweets

                if (tweets == null)
                {
                    return "Twitter user does not exist".SplitInParts(430).ToList();
                }

                if (!tweets.Any())
                {
                    return "No tweets from this user.".SplitInParts(430).ToList();
                }

                var tweet = tweets.ToList()[index - 1];
                return FormatTweet(tweet).SplitInParts(430).ToList();
            }
        }

        internal static string FormatTweet(ITweet tweet, bool showTweetUri = true)
        {
            if (tweet == null)
            {
                return "Tweet not found";
            }

            string tweetUriPart = string.Empty;
            if (showTweetUri)
            {
                tweetUriPart = " - " + tweet.Url;
            }
            var b = IrcConstants.IrcBold;
            var n = IrcConstants.IrcNormal;
            if (tweet.IsRetweet)
            {
                return
                    $"<{b}@{tweet.CreatedBy.ScreenName}>{n}: RT @{tweet.RetweetedTweet.CreatedBy.ScreenName} {WebUtility.HtmlDecode(tweet.RetweetedTweet.FullText.ReplaceNonPrintableCharacters(' ').Replace('\n', ' ').Replace('\r', ' '))} ({tweet.CreatedAt.ToShortDateString()} {tweet.CreatedAt.ToShortTimeString()} UTC; {DateTime.Now.Humanize(false, tweet.CreatedAt)}){tweetUriPart}";            }
            return
                $"<{b}@{tweet.CreatedBy.ScreenName}>{n}: {WebUtility.HtmlDecode(tweet.FullText.ReplaceNonPrintableCharacters(' ').Replace('\n', ' ').Replace('\r', ' '))} ({tweet.CreatedAt.ToShortDateString()} {tweet.CreatedAt.ToShortTimeString()} UTC; {tweet.CreatedAt.Humanize(false, DateTime.Now)}){tweetUriPart}";
        }
    }
}