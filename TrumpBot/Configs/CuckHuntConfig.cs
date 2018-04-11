using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrumpBot.Models;

namespace TrumpBot.Configs
{
    public class CuckHuntConfig : BaseConfig
    {
        public CuckHuntConfig()
        {
            DefaultLocation = "Config\\cuckhunt.json";
            ModelType = typeof(CuckHuntConfigModel.CuckConfig);
        }
    }
}
