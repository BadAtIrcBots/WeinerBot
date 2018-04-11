using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminModel.Right.Admin)]
    internal class RedditStickyAdmin : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(RedditStickyAdmin));
        public string Name { get; } = "RedditSticky";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^reddit_sticky (\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^reddit_sticky (\w+) (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string operation = values[1].Value.ToLower();
            _log.Debug($"StickyAdmin got operation: {operation}");

            if (operation == "stop")
            {
                if (ircBot.RedditSticky.IsAlive())
                {
                    _log.Debug("Stopping RedditSticky thread");
                    ircBot.RedditSticky.Stop();
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Stopped RedditSticky thread");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "RedditSticky thread is not running");
            }
            else if (operation == "start")
            {
                if (!ircBot.RedditSticky.IsAlive())
                {
                    _log.Debug("Starting RedditSticky thread");
                    ircBot.RedditSticky.Start();
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Starting RedditSticky thread");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "RedditSticky thread is running already");
            }
            else if (operation == "status")
            {
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Thread running: {ircBot.RedditSticky.IsAlive()}");
            }
            else if (operation == "reload_config")
            {
                _log.Debug("Reloading config");
                ircBot.RedditSticky.LoadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded config");
            }
            else if (operation == "help")
            {
                List<string> responses = new List<string>
                {
                    "The following comamnds are available: ",
                    "stop -- Stops the thread running",
                    "start -- Starts the thread if it is not running",
                    "status -- gets the status of the thread",
                    "reload_config -- reloads the config from disk",
                    "help -- Display this text"
                };
                foreach (string response in responses)
                {
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, response);
                }
            }

        }
    }
}
