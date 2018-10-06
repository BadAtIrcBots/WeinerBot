using System.Collections.Generic;

namespace TrumpBot.Models.Config
{
    public class ChannelUriConfigModel
    {
        public List<string> EnabledChannels { get; set; } = new List<string>();
        public List<string> TwitterEnabledChannels { get; set; } = new List<string>();
        public List<string> YouTubeEnabledChannels { get; set; } = new List<string>();
        // Does a .Contains(str) check on domain
        public List<string> DomainsToIgnoreDescriptions { get; set; } = new List<string> {"reddit.com"};

        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:62.0) Gecko/20100101 Firefox/62.0";    }
}