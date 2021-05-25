using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    public class RoleUp : IAdminCommand
    {
        private Logger _log = LogManager.GetCurrentClassLogger();

        public string Name { get; } = "RoleUp";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^roleup (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string channel = values[1].Value;

            if (!client.IsJoined(channel))
            {
                _log.Info($"Attempted to roll up {channel}, but not in it.");
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Not in channel {channel}");
                return;
            }
            _log.Info($"Rolling up {channel}");
            List<NonRfcChannelUser> users = client.GetChannel(channel).Users.Values.Cast<NonRfcChannelUser>().Where(user => !user.IsChannelAdmin && !user.IsHalfop && !user.IsOwner && !user.IsVoice).ToList();
            if (users.Count == 0)
            {
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "No users to voice.");
                return;
            }
            string[] usersToVoice = users.Select(user => user.Nick).ToArray();
            _log.Info($"Voicing {usersToVoice.Length} users.");
            client.Voice(channel, usersToVoice);
        }
    }
}