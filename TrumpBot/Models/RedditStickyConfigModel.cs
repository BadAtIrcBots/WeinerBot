using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrumpBot.Models
{
    internal class RedditStickyConfigModel
    {
        public List<string> Channels { get; set; }
        public string Subreddit { get; set; }
        public int CheckInterval { get; set; }
        public string ThingConfigLocation { get; set; } = "Config\\things.json";
        public bool Enabled { get; set; }
    }
}
