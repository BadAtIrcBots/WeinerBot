using System;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public interface IConfig
    {
        string DefaultLocation { get; set; }

        void SaveConfig<T>(T newConfig, string location = null);

        object LoadConfig(string location = null);
        Type ModelType { get; set; }
    }
}
