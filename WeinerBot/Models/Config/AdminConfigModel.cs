using System.Collections.Generic;

namespace WeinerBot.Models.Config
{
    public class AdminConfigModel
    {
        public enum Right
        {
            Guest = 1,
            Moderator = 100,
            Admin = 1000,
            SuperAdmin = 2000
        }

        public class Config : BaseModel
        {
            public List<string> AdminChannels { get; set; }
            public List<User> Users { get; set; }
            public char CommandPrefix { get; set; }
            public string KickMessage { get; set; } = "BTFO";
        }

        public class User
        {
            public string Nick { get; set; }
            public Right Right { get; set; } = Right.Guest;
        }
    }
}
