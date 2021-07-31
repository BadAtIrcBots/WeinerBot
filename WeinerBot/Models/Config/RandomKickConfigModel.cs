using System.Collections.Generic;

namespace WeinerBot.Models.Config
{
    public class RandomKickConfigModel : BaseModel
    {
        public class RandomKickTarget
        {
            public string Nick { get; set; }
            public int Chance { get; set; }
        }
        public List<RandomKickTarget> Targets { get; set; } = new List<RandomKickTarget>();
        public List<string> MessageList { get; set; } = new List<string>();
        public int ChanceThreshold { get; set; } = 1000;
        public bool Enabled { get; set; } = false;
    }
}