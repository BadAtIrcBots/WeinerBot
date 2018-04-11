using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrumpBot.Models
{
    public class WeatherApiConfigModel : BaseModel
    {
        public string ApiKey { get; set; }
        public Dictionary<string, string> UserDefaultLocale { get; set; }
        public bool OnlyDisplayCurrent { get; set; } = false;
        public bool ShortForecast { get; set; } = false;
    }
}
