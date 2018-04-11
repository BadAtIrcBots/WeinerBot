using TrumpBot.Configs;
using TrumpBot.Models;
using Tweetinvi;
using Tweetinvi.Models;

namespace TrumpBot.Services
{
    public static class Twitter
    {
        public static IAuthenticatedUser GetTwitterUser()
        {
            TwitterStreamConfigModel.StreamConfig config =
                new BaseConfig().LoadConfig<TwitterStreamConfigModel.StreamConfig>("Config\\twitter_stream.json");
            Auth.SetUserCredentials(config.ConsumerKey, config.ConsumerSecret, config.AccessToken,
                config.AccessTokenSecret);
            IAuthenticatedUser authenticatedUser = User.GetAuthenticatedUser();
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;
            TweetinviConfig.ApplicationSettings.TweetMode = TweetMode.Extended;
            return authenticatedUser;
        }
    }
}