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
            public bool EnablePongChecking { get; set; } = true;
            // Timeout in ms to give up and restart the bot
            public int PongTimeoutMs { get; set; } = 600000;
            public int PongCheckIntervalMs { get; set; } = 30000;
            public bool TurboPongTimeoutOnDisconnect { get; set; } = true;
            public string BacktraceSubmitUrl { get; set; } = null;
            public string BacktraceToken { get; set; } = null;
            public bool SmartIrc4NetLoggingEnabled { get; set; } = false;
        }
    }
}
