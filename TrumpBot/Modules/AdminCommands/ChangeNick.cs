using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    internal class ChangeNick : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(ChangeNick));

        public string Name { get; } = "ChangeNick";
        public List<Regex> Patterns { get; } = new List<Regex> {new Regex(@"^nick (\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string nick = values[1].Value;
            if (client.Nickname == nick)
            {
                _log.Info($"Tried to change nickname but given nick, {nick}, is already set.");
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Nick is already {nick}");
                return;
            }
            _log.Info($"Changing nick to {nick}");
            client.RfcNick(nick);
        }
    }
}
