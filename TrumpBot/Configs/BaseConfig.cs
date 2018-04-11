using System;
using System.IO;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class BaseConfig : IConfig
    {
        public string DefaultLocation { get; set; } = "Config\\base_config.json";
        public Type ModelType { get; set; } = typeof(BaseModel);
        public void SaveConfig<T>(T newConfig, string location = null)
        {
            if (location == null)
            {
                location = DefaultLocation;
            }
            File.WriteAllText(location, JsonConvert.SerializeObject(newConfig));
        }

        public object LoadConfig(string location = null)
        {
            if (location == null)
            {
                location = DefaultLocation;
            }
            return JsonConvert.DeserializeObject(File.ReadAllText(location), ModelType);
        }

        public T LoadConfig<T>(string location = null)
        {
            if (location == null)
            {
                location = DefaultLocation;
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(location));
        }
    }
}
