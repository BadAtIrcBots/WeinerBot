using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class RedditStickyConfig : BaseConfig
    {
        public RedditStickyConfig()
        {
            DefaultLocation = "Config\\reddit_sticky.json";
            ModelType = typeof(RedditStickyConfigModel);
        }
    }
}
