using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using Meebey.SmartIrc4net;
using WeinerBot.Models.Config;

namespace WeinerBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    internal class PartChannel : IAdminCommand
    {
        private Logger _log = LogManager.GetCurrentClassLogger();

        public string Name { get; } = "PartChannel";
        public List<Regex> Patterns { get; } = new List<Regex> {new Regex(@"^part (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string channel = values[1].Value;

            if (client.IsJoined(channel.TrimStart('#')))
            {
                _log.Info($"Attempted to part {channel}, but not in it");
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Not in channel {channel}");
                return;
            }
            _log.Info($"Parting {channel}");
            client.RfcPart(channel);
        }
    }
}
