using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class IrcConfig : BaseConfig
    {
        public IrcConfig()
        {
            DefaultLocation = "Config\\config.json";
            ModelType = typeof(IrcConfigModel.IrcSettings);
        }
    }
}
