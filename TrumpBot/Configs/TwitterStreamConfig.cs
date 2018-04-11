using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class TwitterStreamConfig : BaseConfig
    {
        public TwitterStreamConfig()
        {
            DefaultLocation = "Config\\twitter_stream.json";
            ModelType = typeof(TwitterStreamConfigModel.StreamConfig);
        }
    }
}
