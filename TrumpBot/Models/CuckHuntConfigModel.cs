using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrumpBot.Models
{
    public class CuckHuntConfigModel
    {
        public class CuckConfig
        {
            public List<string> PhraseList { get; set; }
            public List<string> Channels { get; set; }
            public List<CuckStat> Stats { get; set; }
            public List<int> Random { get; set; }
            public List<string> IgnoreList { get; set; } = new List<string>();
            public List<string> CuckPresentExempt { get; set; } = new List<string>();
            public bool AssumeMaleGender { get; set; } = false;

            public class CuckStat
            {
                public string Nick { get; set; }
                public string Channel { get; set; }
                public int KilledCount { get; set; }
                public int GetEmOutCount { get; set; }
                public int HelicopterCount { get; set; }
            }
        }
    }
}
