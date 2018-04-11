using System;
using System.IO;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    internal class AdminConfig : BaseConfig
    {
        public AdminConfig()
        {
            DefaultLocation = "Config\\admin_config.json";
            ModelType = typeof(AdminModel.Config);
        }
    }
}
