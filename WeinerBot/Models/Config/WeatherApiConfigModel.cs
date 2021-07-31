using System.Collections.Generic;

namespace WeinerBot.Models.Config
{
    public class WeatherApiConfigModel : BaseModel
    {
        public string ApiKey { get; set; }
        public Dictionary<string, string> UserDefaultLocale { get; set; }
        public bool OnlyDisplayCurrent { get; set; } = false;
        public bool ShortForecast { get; set; } = false;
    }
}
