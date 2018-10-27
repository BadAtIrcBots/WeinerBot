using System.Collections.Generic;

namespace TrumpBot.Models.Config
{
    internal class RedditStickyConfigModel
    {
        public List<string> Channels { get; set; }
        public string Subreddit { get; set; }
        public int CheckInterval { get; set; }
        public string ThingConfigLocation { get; set; } = "Config\\things.json";
        public bool Enabled { get; set; }
        public bool IgnoreTrumpTweetReposts { get; set; } = true;
    }
}
