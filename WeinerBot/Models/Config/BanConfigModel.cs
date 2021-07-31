using System;

namespace WeinerBot.Models.Config
{
    public class BanConfigModel
    {
        public class Config
        {
            public string DefaultBanMessage { get; set; } = "OUT OUT OUT";
            public int DefaultBanLength { get; set; } = 30; // Minutes
        }

        public class Ban
        {
            public string Nick { get; set; }
            public string Mask { get; set; }
            public DateTime BanDate { get; set; }
            public string Channel { get; set; }
            public int BanLength { get; set; }
            public bool Expired { get; set; }
            public int RemainingBanLength { get; set; }
        }
    }
}