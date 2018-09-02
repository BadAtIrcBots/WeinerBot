using TrumpBot.Configs;
using TrumpBot.Models.Config;
using Tweetinvi;
using Tweetinvi.Models;

namespace TrumpBot.Services
{
    public static class Twitter
    {
        public static IAuthenticatedUser GetTwitterUser()
        {
            TwitterStreamConfigModel.StreamConfig config =
                ConfigHelpers.LoadConfig<TwitterStreamConfigModel.StreamConfig>(ConfigHelpers.ConfigPaths
                    .TwitterStreamConfig);
            Auth.SetUserCredentials(config.ConsumerKey, config.ConsumerSecret, config.AccessToken,
                config.AccessTokenSecret);
            IAuthenticatedUser authenticatedUser = User.GetAuthenticatedUser();
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;
            TweetinviConfig.ApplicationSettings.TweetMode = TweetMode.Extended;
            return authenticatedUser;
        }
    }
}