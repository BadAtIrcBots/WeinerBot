using System;
using System.Collections.Generic;

namespace TrumpBot.Models.Config
{
    public class IrcConfigModel
    {
        public class IrcSettings : BaseModel
        {
            public Uri ConnectionUri { get; set; }
            public string RealName { get; set; } = null;
            public string NickservPassword { get; set; } = null;
            public string Nick { get; set; } = null;
            public string Username { get; set; } = null;
            public bool AutoRestart { get; set; } = true;
            public List<string> AutoJoinChannels { get; set; }
            public List<string> JoinProtectedChannels { get; set; } = new List<string>();
            public List<string> Admins { get; set; } = new List<string>();
            public string RavenDsn { get; set; } = null;
        }
    }
}
