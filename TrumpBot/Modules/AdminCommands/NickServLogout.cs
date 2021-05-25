using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;
using TrumpBot.Services;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    internal class NickServLogout : IAdminCommand
    {
        private Logger _log = LogManager.GetCurrentClassLogger();

        public string Name { get; } = "NickServLogout";
        public List<Regex> Patterns { get; } = new List<Regex> {new Regex(@"^nickserv logout$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            _log.Info("Logging out of NickServ");
            new NickServ(client, ircBot.Settings).Logout();
        }
    }
}
