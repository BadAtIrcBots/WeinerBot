using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using TrumpBot.Configs;
using TrumpBot.Models.Config;

namespace TrumpBot.Services
{
    public static class UrlHistory
    {
        public static int CacheExpiration = 30;
        public static StringComparison ComparisonCulture = StringComparison.CurrentCultureIgnoreCase;
        public static string CacheName = "UrlHistoryCache";
        public static string FormatResponse(UrlHistoryConfigModel.UrlMention mention, string nick, string template, bool preventHighlight = false)
        {
            string mentionUser = mention.User;
            if (preventHighlight)
            {
                // \u2063 is an invisible whitespace, any client that is not discarding unicode should handle this
                mentionUser = mentionUser.Insert(mentionUser.Length / 1, "\u2063").Insert(1, "\u2063");
            }
            
            return template.Replace("{MentionUser}", mentionUser).Replace("{Nick}", nick)
                .Replace("{Url}", mention.Link).Replace("{TimeAgo}", mention.Timestamp.Humanize(true, DateTime.UtcNow));
        }

        public static string GetResponse(UrlHistoryConfigModel.UrlHistoryConfig config, string channel)
        {
            List<string> possibleMentions =
                config.DefaultResponses.Concat(config.ChannelConfigs
                        .Find(c => string.Equals(c.Channel, channel, ComparisonCulture))
                        .Responses)
                    .ToList();
            return possibleMentions[new Random().Next(0, possibleMentions.Count - 1)];
        }

        // null if no mention
        public static UrlHistoryConfigModel.UrlMention GetUrlMention(UrlHistoryConfigModel.UrlHistoryConfig config, string channel, string url)
        {
            // Links in stored mentions should not contain a #, clean up any imports you might do, regex I used to match # in links was "#(\S+)"
            // It's mostly a preference to not store #blahblah stuff as it is client specific and may cause false negatives for people posting very similar URLs
            if (url.Contains("#"))
            {
                url = url.Substring(0, url.LastIndexOf("#", StringComparison.InvariantCulture) + 1);
            }

            var channelConfig = config.ChannelConfigs.Find(c =>
                                    string.Equals(c.Channel, channel, ComparisonCulture)) ??
                                new UrlHistoryConfigModel.ChannelConfig {MatchMentionsFromOtherChannels = false};

            var mentionMatch = config.Mentions.Find(m =>
                m.Link == url && (string.Equals(channel, m.Channel, ComparisonCulture) ||
                                  channelConfig.MatchMentionsFromOtherChannels));
            return mentionMatch;
        }

        // Unless there are external edits to the url_history.json file, the cache should be in sync with the filesystem
        public static UrlHistoryConfigModel.UrlHistoryConfig GetConfig(bool preferCache = true)
        {
            var config = Cache.Get<UrlHistoryConfigModel.UrlHistoryConfig>(CacheName);
            if (config == null || !preferCache)
            {
                config = ConfigHelpers.LoadConfig<UrlHistoryConfigModel.UrlHistoryConfig>(ConfigHelpers
                    .ConfigPaths.UrlHistoryConfig);
                Cache.Set(CacheName, config, DateTimeOffset.Now.AddMinutes(CacheExpiration));
            }
            return config;
        }

        public static void SaveConfig(UrlHistoryConfigModel.UrlHistoryConfig config, bool syncToCache = true)
        {
            if (syncToCache)
            {
                Cache.Set(CacheName, config, DateTimeOffset.Now.AddMinutes(CacheExpiration));
            }
            ConfigHelpers.SaveConfig(config, ConfigHelpers.ConfigPaths.UrlHistoryConfig);
        }

        public static void AddUrlMention(string nick, string channel, string url, bool autosave = true)
        {
            if (url.Contains("#"))
            {
                url = url.Substring(0, url.LastIndexOf("#", StringComparison.InvariantCulture) + 1);
            }
            
            var config = GetConfig();
            config.Mentions.Add(new UrlHistoryConfigModel.UrlMention
            {
                Channel = channel.ToLower(),
                Link = url,
                Timestamp = DateTime.UtcNow,
                User = nick
            });
            if (!autosave)
            {
                return;
            }
            SaveConfig(config);
        }

        public static bool ShouldPreventNickHighlight(string nick)
        {
            return GetConfig().PreventNickHighlightList.Contains(nick);
        }

        public static bool IsChannelEnabled(string channel)
        {
            var config = GetConfig();
            var channelConfig = config.ChannelConfigs.Find(c => c.Channel == channel);
            return channelConfig != null && channelConfig.Enabled;
        }
    }
}