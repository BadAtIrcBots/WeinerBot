using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    public class ConfigAdmin : IAdminCommand
    {
        public string Name { get; } = "ManageConfig";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^config (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        private ILog _log = LogManager.GetLogger(typeof(ConfigAdmin));

        [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string operation = values[1].Value;

            if (operation == "reload_cuckhunt")
            {
                _log.Debug("Reloading cuckhnt");
                ircBot.CuckHunt.ReloadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded cuckhunt config (this will only affect messages)");
            }
            else if (operation == "reload_admin")
            {
                _log.Debug("Reloading admin config");
                ircBot.Admin.ReloadAdminConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded admin config");
            }
        }
    }
}
