using System.Collections.Generic;

namespace TrumpBot.Models.Config
{
    public class TwitterStreamConfigModel
    {
        public class StreamConfig : BaseModel
        {
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
            public string AccessToken { get; set; }
            public string AccessTokenSecret { get; set; }
            public bool Enabled { get; set; } = true;
            public List<Stream> Streams { get; set; }
            public bool EnableTtrpm { get; set; } = true;
        }

        public class Stream
        {
            public long TwitterUserId { get; set; }
            public List<string> Channels { get; set; }
            public bool IgnoreRetweets { get; set; } = false;
            public bool IgnoreReplies { get; set; } = false;
            public List<long> RetweetsToIgnoreByUserId { get; set; } = new List<long>();
        }
    }
}
