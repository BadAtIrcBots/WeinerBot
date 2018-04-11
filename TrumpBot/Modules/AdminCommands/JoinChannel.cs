using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminModel.Right.Admin)]
    internal class JoinChannel : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(JoinChannel));

        public string Name { get; } = "JoinChannel";
        public List<Regex> Patterns { get; } = new List<Regex> {new Regex(@"^join (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string channel = values[1].Value;
            if (client.IsJoined(channel))
            {
                _log.Info($"Attempted to join {channel} but already in it");
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Already in channel {channel}");
                return;
            }
            _log.Info($"Joining {channel}");
            client.RfcJoin(channel);
        }
    }
}
