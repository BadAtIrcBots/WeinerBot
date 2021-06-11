using TrumpBot.Configs;
using TrumpBot.Models.Config;
using Tweetinvi;

namespace TrumpBot.Services
{
    public static class Twitter
    {
        public static TwitterClient GetTwitterClient()
        {
            TwitterStreamConfigModel.StreamConfig config =
                ConfigHelpers.LoadConfig<TwitterStreamConfigModel.StreamConfig>(ConfigHelpers.ConfigPaths
                    .TwitterStreamConfig);
            if (!config.Enabled)
            {
                return null;
            }
            var twitterClient = new TwitterClient(config.ConsumerKey, config.ConsumerSecret, config.AccessToken,
                config.AccessTokenSecret);
            twitterClient.Config.TweetMode = TweetMode.Extended;
            return twitterClient;
        }
    }
}