using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminModel.Right.Admin)]
    internal class ToggleCache : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(ToggleCache));

        public string Name { get; } = "ToggleCache";
        public List<Regex> Patterns { get; } = new List<Regex> {new Regex(@"^togglecache$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            ircBot.UseCache = !ircBot.UseCache;
            _log.Info($"useCache is now {ircBot.UseCache}");
            client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"useCache is now {ircBot.UseCache}");
        }
    }
}
