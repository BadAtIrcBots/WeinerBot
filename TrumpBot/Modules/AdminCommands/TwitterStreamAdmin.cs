using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    public class TwitterStreamAdmin
    {
        [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
        internal class TwitterStreamReloadConfigAdminCommand : IAdminCommand
        {
            private ILog _log = LogManager.GetLogger(typeof(TwitterStreamReloadConfigAdminCommand));
            public string Name { get; } = "TwitterStreamReloadConfigAdminCommand";
            public List<Regex> Patterns { get; } = new List<Regex>
            {
                new Regex(@"^twitter reload$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^tw reload$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
            {
                _log.Debug($"Reloading Twitter Stream config for {eventArgs.Data.From}");
                ircBot.TwitterStream.LoadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded config successfully, the bot will not follow any new user IDs but other changes will take effect.");
            }
        }
    }
}