using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class DebateConfig : IConfig
    {
        public string DefaultLocation { get; set; } = "Config\\debates.json";

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
            List<DebateModel.Debate> debates =
                JsonConvert.DeserializeObject<List<DebateModel.Debate>>(File.ReadAllText(location));
            return new DebateContainer
            {
                Debates = debates
            };
        }

        public Type ModelType { get; set; }

        internal class DebateContainer : BaseModel
        {
            internal List<DebateModel.Debate> Debates { get; set; }
        }
    }
}
