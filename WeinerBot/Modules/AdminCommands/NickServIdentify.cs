using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using Meebey.SmartIrc4net;
using WeinerBot.Models.Config;
using WeinerBot.Services;

namespace WeinerBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    internal class NickServIdentify : IAdminCommand
    {
        private Logger _log = LogManager.GetCurrentClassLogger();

        public string Name { get; } = "NickServIdentify";
        public List<Regex> Patterns { get; } = new List<Regex> {new Regex(@"^nickserv identify$", RegexOptions.Compiled | RegexOptions.IgnoreCase)};

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            _log.Info("Identifying with NickServ");
            try
            {
                new NickServ(client, ircBot.Settings).Identify();
            }
            catch (NickServ.NoNickServPasswordException e)
            {
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, e.Message);
            }
        }
    }
}
