using System.IO;
using Newtonsoft.Json;

namespace WeinerBot.Configs
{
    public static class ConfigHelpers
    {
        public static void SaveConfig(object newConfig, string location)
        {
            File.WriteAllText(location, JsonConvert.SerializeObject(newConfig, Formatting.Indented));
        }

        public static T LoadConfig<T>(string location)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(location));
        }

        public static class ConfigPaths
        {
            public static string CommandConfig => Path.Join("Config", "command_config.json");
            public static string CuckHuntConfig => Path.Join("Config", "cuckhunt.json");
            public static string AdminConfig => Path.Join("Config", "admin_config.json");
            public static string IrcConfig => Path.Join("Config", "config.json");
            public static string TwitterStreamConfig => Path.Join("Config", "twitter_stream.json");
            public static string WeatherApiConfig => Path.Join("Config", "weather_api.json");
            public static string RandomKickConfig => Path.Join("Config", "random_kick.json");
            public static string YouTubeApiConfig => Path.Join("Config", "youtube_api.json");
            public static string UrlHistoryConfig => Path.Join("Config", "url_history.json");
            public static string ChannelUriConfig => Path.Join("Config", "uri.json");
            public static string AirportConfig => Path.Join("Config", "airports.json");
            public static string IexApiConfig => Path.Join("Config", "iex_api.json");
        }
    }
}
