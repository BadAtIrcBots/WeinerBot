using System;
using System.Runtime.Caching;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    internal class Reddit
    {
        internal RedditModel.SubredditModel.Subreddit GetSubreddit(string subreddit, bool useCache, int seconds = 300)
        {
            if (!useCache)
            {
                return _getSubreddit(subreddit);
            }

            ObjectCache cache = MemoryCache.Default;
            string cacheObjectName = $"Subreddit-{subreddit}";
            RedditModel.SubredditModel.Subreddit subredditData = cache[cacheObjectName] as RedditModel.SubredditModel.Subreddit;

            if(subredditData != null) return subredditData;

            subredditData = _getSubreddit(subreddit);
            cache.Set(cacheObjectName, subredditData, new DateTimeOffset(DateTime.Now.AddSeconds(seconds)));

            return subredditData;
        }

        private RedditModel.SubredditModel.Subreddit _getSubreddit(string subreddit)
        {
            return
                JsonConvert.DeserializeObject<RedditModel.SubredditModel.Subreddit>(
                    Http.GetJson(new Uri($"https://www.reddit.com/r/{subreddit}.json")));
        }
    }
}
