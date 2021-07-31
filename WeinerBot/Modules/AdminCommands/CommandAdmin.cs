using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using NLog;
using WeinerBot.Configs;
using WeinerBot.Models.Config;

namespace WeinerBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    internal class CommandAdmin : IAdminCommand
    {
        private Logger _log = LogManager.GetCurrentClassLogger();
        public string Name { get; } = "CommandAdmin";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^commands (\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^commands (\w+) (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string operation = values[1].Value;
            _log.Debug($"Got operation: {operation}");
            string argument = null;

            if (values.Count > 2)
            {
                argument = values[2].Value;
            }

            if (operation == "ignore")
            {
                if (argument != null)
                {
                    CommandConfigModel config =
                        ConfigHelpers.LoadConfig<CommandConfigModel>(ConfigHelpers.ConfigPaths.CommandConfig);
                    if (config.IgnoreList.Contains(argument))
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Nick already ignored");
                        return;
                    }
                    config.IgnoreList.Add(argument);
                    ConfigHelpers.SaveConfig(config, ConfigHelpers.ConfigPaths.CommandConfig);
                    ircBot.Command.LoadConfig();
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Successfully ignored {argument}. Currently ignoring: {string.Join(", ", config.IgnoreList)}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "unignore")
            {
                if (argument != null)
                {
                    CommandConfigModel config =
                        ConfigHelpers.LoadConfig<CommandConfigModel>(ConfigHelpers.ConfigPaths.CommandConfig);
                    if (config.IgnoreList.Contains(argument))
                    {
                        config.IgnoreList.Remove(argument);
                        ConfigHelpers.SaveConfig(config, ConfigHelpers.ConfigPaths.CommandConfig);
                        ircBot.Command.LoadConfig();
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Successfully unignored {argument}. Currently ignoring: {string.Join(", ", config.IgnoreList)}");
                        return;
                    }
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"{argument} is not in the ignore list. Currently ignoring: {string.Join(", ", config.IgnoreList)}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "ignore_list" || operation == "ignorelist")
            {
                CommandConfigModel config =
                    ConfigHelpers.LoadConfig<CommandConfigModel>(ConfigHelpers.ConfigPaths.CommandConfig);
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Currently ignoring: {string.Join(", ", config.IgnoreList)}");
            }
            else if (operation == "rehash")
            {
                ircBot.Command.LoadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Module configuration rehashed");
            }
            else if (operation == "toggle_voice_only")
            {
                CommandConfigModel config =
                    ConfigHelpers.LoadConfig<CommandConfigModel>(ConfigHelpers.ConfigPaths.CommandConfig);
                config.IgnoreNonVoicedUsers = !config.IgnoreNonVoicedUsers;
                ConfigHelpers.SaveConfig(config, ConfigHelpers.ConfigPaths.CommandConfig);
                ircBot.Command.LoadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Voice only is now: {config.IgnoreNonVoicedUsers}");
            }
            else if (operation == "help")
            {
                List<string> responses = new List<string>
                {
                    "The following commands are available: ",
                    "ignore <nick> -- Ignores a nick for all command module related functionality",
                    "unignore <nick> -- Unignores a nick and flushes config to disk immediately",
                    "ignore_list | ignorelist -- Prints out the list of ignored links",
                    "help -- Displays this text"
                };

                foreach (string response in responses)
                {
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, response);
                }
            }
        }
    }
}
