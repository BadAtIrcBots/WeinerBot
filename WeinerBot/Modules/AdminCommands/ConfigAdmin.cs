using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using NLog;
using WeinerBot.Models.Config;

namespace WeinerBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    public class ConfigAdmin : IAdminCommand
    {
        public string Name { get; } = "ManageConfig";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^config (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        private Logger _log = LogManager.GetCurrentClassLogger();

        [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string operation = values[1].Value;

            if (operation == "reload_cuckhunt")
            {
                _log.Info("Reloading cuckhunt");
                ircBot.CuckHunt.ReloadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded cuckhunt config (this will only affect messages)");
            }
            else if (operation == "reload_admin")
            {
                _log.Info("Reloading admin config");
                ircBot.Admin.ReloadAdminConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded admin config");
            }
        }
    }
}
