using System;
using System.IO;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public static class ConfigHelpers
    {
        public static void SaveConfig(object newConfig, string location)
        {
            File.WriteAllText(location, JsonConvert.SerializeObject(newConfig));
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
            public static string DebateConfig { get; } = "Config\\debates.json";
            public static string ElectionDateTimeConfig { get; } = "Config\\election_date.json";
            public static string IrcConfig { get; } = "Config\\config.json";
            public static string RedditStickyConfig { get; } = "Config\\reddit_sticky.json";
            public static string TwitterStreamConfig { get; } = "Config\\twitter_stream.json";
            public static string WeatherApiConfig { get; } = "Config\\weather_api.json";
            public static string RandomKickConfig { get; } = "Config\\random_kick.json";
            public static string AlabamaElectionConfig { get; } = "Config\\al_election.json";
            public static string TetherConfig { get; } = "Config\\tether_config.json";
        }
    }
}
