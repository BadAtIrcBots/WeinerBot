using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class CommandConfig : BaseConfig
    {
        public CommandConfig()
        {
            DefaultLocation = "Config\\command_config.json";
            ModelType = typeof(CommandConfigModel);
        }
    }
}
