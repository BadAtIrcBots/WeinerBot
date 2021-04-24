using System.IO;
using Newtonsoft.Json;

namespace TrumpBot.Configs
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
            public static string CommandConfig { get; } = "Config\\command_config.json";
            public static string CuckHuntConfig { get; } = "Config\\cuckhunt.json";
            public static string AdminConfig { get; } = "Config\\admin_config.json";
            public static string IrcConfig { get; } = "Config\\config.json";
            public static string TwitterStreamConfig { get; } = "Config\\twitter_stream.json";
            public static string WeatherApiConfig { get; } = "Config\\weather_api.json";
            public static string RandomKickConfig { get; } = "Config\\random_kick.json";
            public static string YouTubeApiConfig { get; } = "Config\\youtube_api.json";
            public static string UrlHistoryConfig { get; } = "Config\\url_history.json";
            public static string ChannelUriConfig { get; } = "Config\\uri.json";
            public static string AirportConfig { get; } = "Config\\airports.json";
            public static string IexApiConfig { get; } = "Config\\iex_api.json";
        }
    }
}
