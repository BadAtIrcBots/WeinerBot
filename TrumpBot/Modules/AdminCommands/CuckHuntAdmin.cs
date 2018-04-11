using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Models;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminModel.Right.Admin)]
    internal class CuckHuntAdmin : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(CuckHuntAdmin));
        public string Name { get; } = "CuckHunt";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^cuckhunt (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^cuckhunt (\S+) (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^cuckhunt (\S+) (\S+) (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^cuckhunt (\S+) (\S+) (\S+) (\S+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            string operation = values[1].Value.ToLower();
            _log.Debug($"Got operation: {operation}");

            if (operation == "spawn")
            {
                if (values.Count > 2)
                {
                    string channel = values[2].Value;
                    _log.Debug($"Spawning cuck in {channel}");
                    if (!ircBot.CuckHunt.IsCuckPresent(channel))
                    {
                        ircBot.CuckHunt.CreateCuck(channel, manuallyCreated: true);
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Spawned cuck in {channel}");
                        return;
                    }
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Cuck is already present in {channel}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "list")
            {
                List<CuckHunt.Cuck> cucks = ircBot.CuckHunt.GetCucks();
                _log.Debug($"Got {cucks}");
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"{cucks.Count} cuck(s) present");
                foreach (CuckHunt.Cuck cuck in cucks)
                {
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Cuck in {cuck.Channel}, appeared at: {cuck.Appeared.ToLongTimeString()}");
                }
            }
            else if (operation == "destroy")
            {
                if (values.Count > 2)
                {
                    string channel = values[2].Value;
                    _log.Debug($"Destroying cuck in {channel}");
                    if (ircBot.CuckHunt.IsCuckPresent(channel))
                    {
                        ircBot.CuckHunt.RemoveCuck(channel);
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Destroyed cuck in {channel}");
                        return;
                    }
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Cuck not present in {channel}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "start_hunt")
            {
                if (values.Count > 2)
                {
                    string channel = values[2].Value;

                    if (channel[0] != '#')
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                            "Second parameter must start with #");
                        return;
                    }

                    if (ircBot.CuckHunt.IsCuckHuntActive(channel))
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"The hunt is already active in {channel}");
                        return;
                    }

                    ircBot.CuckHunt.StartHunt(channel);

                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Enabled hunt in {channel}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "stop_hunt")
            {
                if (values.Count > 2)
                {
                    string channel = values[2].Value;

                    if (channel[0] != '#')
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                            "Second parameter must start with #");
                        return;
                    }

                    if (!ircBot.CuckHunt.IsCuckHuntActive(channel))
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Hunt is already inactive in {channel}");
                        return;
                    }
                    ircBot.CuckHunt.DestroyThread(channel);
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Hunt is now disabled in {channel}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "ignore")
            {
                if (values.Count > 2)
                {
                    string nick = values[2].Value;

                    ircBot.CuckHunt.Ignore(nick);
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Successfully ignoring {nick}. Currently ignoring: {string.Join(", ", ircBot.CuckHunt.GetIgnoreList())}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "unignore")
            {
                if (values.Count > 2)
                {
                    string nick = values[2].Value;
                    ircBot.CuckHunt.UnIgnore(nick);
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Successfully unignored {nick}. Currently ignoring: {string.Join(", ", ircBot.CuckHunt.GetIgnoreList())}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Second parameter required");
            }
            else if (operation == "ignore_list" || operation == "ignorelist")
            {
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Currently ignored nicks: {string.Join(", ", ircBot.CuckHunt.GetIgnoreList())}");
            }
            else if (operation == "rehash")
            {
                ircBot.CuckHunt.ReloadConfig();
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Successfully rehashed the cuckhunt configuration");
            }
            else if (operation == "help")
            {
                List<string> responses = new List<string>
                {
                    "The following commands are available: ",
                    "spawn <channel> -- Spawns a cuck in the given channel",
                    "list -- Gives a list of all cucks that are currently active",
                    "destroy <channel> -- Destroys a cuck in the channel",
                    "start_hunt <channel> -- Begins a hunt in a channel (spawns thread, begins hunt, use 'spawn' command if you need a cuck present immediately after enabling. Does not save to config, will be lost on bot restart!)",
                    "stop_hunt <channel> -- Stops an active hunt in a channel (destroys thread, does not save to config, hunt will restart if in 'Channels' property on config)",
                    "ignore <nick> -- Ignore a nickname when it attempts to !getout or !deport",
                    "unignore <nick> -- Remove a nickname from the ignore list",
                    "ignore_list | ignorelist -- Get a list of all ignored nicks",
                    "merge_stats | merge_stats_ignore_missing_new_user <channel> <old> <new> -- Move old user's scores to new user (resets old user's scores to 0)",
                    "stats <channel> <user> -- Display user's stats",
                    "help -- Display this text"
                };

                foreach (string response in responses)
                {
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, response);
                }
            }
            else if (operation == "merge_scores" || operation == "mergescores" || operation == "merge_scores_ignore_missing_new_user" || operation == "merge_stats" || operation == "merge_stats_ignore_missing_new_user")
            {
                if (values.Count > 4)
                {
                    bool ignoreMissingNewUser = operation == "merge_scores_ignore_missing_new_user" || operation == "merge_stats_ignore_missing_new_user";
                    string channel = values[2].Value;
                    string oldUser = values[3].Value;
                    string newUser = values[4].Value;
                    CuckHuntConfigModel.CuckConfig.CuckStat oldUserStats =
                        ircBot.CuckHunt.GetCuckStat(oldUser, channel);
                    CuckHuntConfigModel.CuckConfig.CuckStat newUserStats =
                        ircBot.CuckHunt.GetCuckStat(newUser, channel);

                    if (oldUserStats == null)
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Old user not found");
                        return;
                    }
                    if (newUserStats == null && !ignoreMissingNewUser)
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, "New user not found, change operation to 'merge_stats_ignore_missing_new_user' to bypass this check");
                        return;
                    }

                    client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                        $"Old user's stats: Channel={oldUserStats.Channel}; Nick={oldUserStats.Nick}; GetEmOutCount={oldUserStats.GetEmOutCount}; KilledCount={oldUserStats.KilledCount}; HelicopterCount={oldUserStats.HelicopterCount}");
                    if (newUserStats != null)
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                            $"New user's stats: Channel={newUserStats.Channel}; Nick={newUserStats.Nick}; GetEmOutCount={newUserStats.GetEmOutCount}; KilledCount={newUserStats.KilledCount}; HelicopterCount={newUserStats.HelicopterCount}");
                        ircBot.CuckHunt.SetScore(newUser, channel, oldUserStats.GetEmOutCount + newUserStats.GetEmOutCount, oldUserStats.KilledCount + newUserStats.KilledCount, oldUserStats.HelicopterCount + newUserStats.HelicopterCount, createIfNotExists: true);

                    }
                    else
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, "New user is null, will be created when scores are set.");
                        ircBot.CuckHunt.SetScore(newUser, channel, oldUserStats.GetEmOutCount, oldUserStats.KilledCount, oldUserStats.HelicopterCount, createIfNotExists: true);

                    }

                    ircBot.CuckHunt.SetScore(oldUser, channel, 0, 0, 0);
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Stats merged and saved");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Not enough arguments");
            }
            else if (operation == "stats" || operation == "get_stats")
            {
                if (values.Count > 3)
                {
                    string channel = values[2].Value;
                    string nick = values[3].Value;
                    CuckHuntConfigModel.CuckConfig.CuckStat stats = ircBot.CuckHunt.GetCuckStat(nick, channel);
                    if (stats == null)
                    {
                        client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Could not find the user");
                        return;
                    }
                    client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"User's stats: Channel={stats.Channel}; Nick={stats.Nick}; GetEmOutCount={stats.GetEmOutCount}; KilledCount={stats.KilledCount}; HelicopterCount={stats.HelicopterCount}");
                    return;
                }
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Not enough arguments");
            }
        }
    }
}
