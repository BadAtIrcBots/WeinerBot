using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Meebey.SmartIrc4net;
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

        [Admin.RequiredRight(AdminConfigModel.Right.Moderator)]
        internal class KickAdminRegexCommand : IAdminCommand
        {
            public string Name { get; } = "KickAdminRegexCommand";

            public List<Regex> Patterns { get; } = new List<Regex>
            {
                new Regex(@"^kickregex (\S+) (\S+)$"),
                new Regex(@"^kickregex (\S+) (\S+) (\S+)$"),
                new Regex(@"^kickregexwhatif (\S+) (\S+)$"),
                new Regex(@"^kickregexwhatif (\S+) (\S+) (\S+)$")
                
            };
            public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
            {
                string regexPattern;
                string reason = "Kicked";
                string channel;
                bool whatif = false;
                
                if (values.Count >= 3)
                {
                    channel = values[1].Value;
                    regexPattern = values[2].Value;
                    if (values.Count == 4)
                    {
                        reason = values[3].Value;
                    }
                }
                else
                {
                    channel = eventArgs.Data.Channel;
                    regexPattern = values[1].Value;
                }

                if (eventArgs.Data.Message.Contains("kickregexwhatif"))
                {
                    whatif = true;
                }

                var regex = new Regex(regexPattern);
                List<string> kickList = client.GetChannel(channel).Users.Keys.Cast<string>().Where(user => regex.Match(user).Success).ToList();

                int cooldown = 200; // Ms between kick waves
                int waveSize = 4;

                if (whatif)
                {
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Would've kicked {kickList.Count} users in {channel} for '{reason}' reason");
                    return;
                }
                
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Kicking {kickList.Count} users in {channel} for '{reason}' reason");

                for (var i = 0; i < kickList.Count; i += waveSize)
                {
                    var nicks = kickList.Skip(i).Take(waveSize); 
                    client.RfcKick(channel, nicks.ToArray(), reason);
                    Thread.Sleep(cooldown);
                }
            }
        }
    }
}