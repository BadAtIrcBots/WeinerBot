using System;
using System.Collections.Generic;

namespace WeinerBot.Models.Config
{
    public class UrlHistoryConfigModel
    {
        public class UrlMention
        {
            public string User { get; set; }
            public string Link { get; set; }
            public DateTime Timestamp { get; set; }
            public string Channel { get; set; }
        }

        public class ChannelConfig
        {
            public string Channel { get; set; } = string.Empty;
            public bool Enabled { get; set; } = false;
            public List<string> Responses { get; set; } = new List<string>();
            public bool MatchMentionsFromOtherChannels { get; set; } = false;
        }

        public class UrlHistoryConfig
        {
            public List<UrlMention> Mentions { get; set; } = new List<UrlMention>();
            public List<ChannelConfig> ChannelConfigs { get; set; } = new List<ChannelConfig>();
            public List<string> DefaultResponses { get; set; } = new List<string>();
            public List<string> PreventNickHighlightList { get; set; } = new List<string>();
        }
    }
}