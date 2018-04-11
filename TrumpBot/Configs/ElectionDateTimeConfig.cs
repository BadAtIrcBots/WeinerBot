using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class ElectionDateTimeConfig : BaseConfig
    {
        public ElectionDateTimeConfig()
        {
            DefaultLocation = "Config\\election_date.json";
            ModelType = typeof(ElectionDateTimeConfigModel);
        }
    }
}
