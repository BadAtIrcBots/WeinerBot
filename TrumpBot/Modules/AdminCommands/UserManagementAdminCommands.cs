using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    internal class UserManagementAdminCommands
    {
        [Admin.RequiredRight(AdminConfigModel.Right.Moderator)]
        internal class KickAdminCommand : IAdminCommand
        {
            private ILog _log = LogManager.GetLogger(typeof(KickAdminCommand));
            public string Name { get; } = "KickAdminCommand";
            public List<Regex> Patterns { get; } = new List<Regex>
            {
                new Regex(@"^kick (\S+) (\S+) (.*)$"),
                new Regex(@"^kick (\S+) (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^kick (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
            {
                string target;
                string channel;
                string reason = ircBot.Admin.Config.KickMessage;
                
                if (values.Count >= 3)
                {
                    channel = values[1].Value;
                    target = values[2].Value;
                    if (values.Count == 4)
                    {
                        reason = values[3].Value;
                    }
                }
                else
                {
                    channel = eventArgs.Data.Channel;
                    target = values[1].Value;
                }
                
                _log.Info($"User {eventArgs.Data.From} has asked the bot to kick {target} in {channel}");

                if (!client.IsJoined(channel))
                {
                    _log.Info($"Cannot kick {target} as the bot is not in {channel}");
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Not in channel {channel}");
                }
                client.RfcKick(channel, target, reason);
            }
        }
    }
}