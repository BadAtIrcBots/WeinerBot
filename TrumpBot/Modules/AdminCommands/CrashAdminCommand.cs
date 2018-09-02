using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    [Admin.IgnoreException]
    internal class CrashAdminCommand : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(CrashAdminCommand));
        public string Name { get; } = "Crash";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^crash$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            _log.Debug("Crashing IRC bot");
            throw new Exception("This was deliberately caused by the crash admin command.");
        }
    }
}
