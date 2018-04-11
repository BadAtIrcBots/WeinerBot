using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrumpBot.Configs
{
    public class WeatherApiConfig : BaseConfig
    {
        public WeatherApiConfig()
        {
            DefaultLocation = "Config\\weather_api.json";
            ModelType = typeof(Models.WeatherApiConfigModel);
        }
    }
}
