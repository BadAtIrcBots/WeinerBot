﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Backtrace;
using NLog;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using WeinerBot.Configs;
using WeinerBot.Models.Config;

namespace WeinerBot.Modules
{
    public class CuckHunt
    {
        private IrcClient _client;
        private IrcBot _ircBot;
        private CuckHuntConfigModel.CuckConfig _config;
        private List<CuckThread> _threads = new List<CuckThread>();
        private Logger _log = LogManager.GetCurrentClassLogger();
        private List<Cuck> _cucks = new List<Cuck>();
        private DateTimeOffset? _lastDeportedCuck = null;
        private BacktraceClient _backtraceClient = Services.Backtrace.GetBacktraceClient();
        public string LastAdminWhoSpawnedCuck = string.Empty;
        public Dictionary<string, ActivityTime> LastActivity = new Dictionary<string, ActivityTime>();

        public char CommandPrefix;


        public CuckHunt(IrcClient client, IrcBot bot)
        {
            _client = client;
            _ircBot = bot;
            ReloadConfig();
            CommandPrefix = _ircBot.Command.CommandPrefix;
            foreach (string channel in _config.Channels)
            {
                CreateThread(channel);
            }
        }

        ~CuckHunt()
        {
            _log.Debug("Disposing all threads in CuckHunt");
            foreach (CuckThread thread in _threads)
            {
                DestroyThread(thread.Channel);
            }
        }

        public void SaveConfig()
        {
            ConfigHelpers.SaveConfig(_config, ConfigHelpers.ConfigPaths.CuckHuntConfig);
        }

        public void Ignore(string nick)
        {
            _config.IgnoreList.Add(nick);
            SaveConfig();
        }

        public void UnIgnore(string nick)
        {
            if (!_config.IgnoreList.Contains(nick)) return;

            _config.IgnoreList.Remove(nick);
            SaveConfig();
        }

        public List<string> GetIgnoreList()
        {
            return _config.IgnoreList;
        }

        public void ClearCucks()
        {
            _log.Debug("Erasing all active cucks");
            _cucks.Clear();
        }

        public void ReloadConfig()
        {
            _config = ConfigHelpers.LoadConfig<CuckHuntConfigModel.CuckConfig>(ConfigHelpers.ConfigPaths
                .CuckHuntConfig);
        }

        public void CuckThreadCallback(string channel, int sleepTime, CancellationToken cancellationToken)
        {
            _log.Debug($"{channel} cuck thread sleeping for {sleepTime / 1000} seconds");
            Thread.Sleep(sleepTime);
            if (cancellationToken.IsCancellationRequested)
            {
                _log.Debug("CuckThread woke up to find it has now been cancelled. Sad :(");
                return;
            }
            if (IsCuckPresent(channel)) // Only possible if someone is fucking with the admin tools now
            {
                _log.Debug($"Cuck already present in {channel}, skipping");
                return;
            }
            if (IsCuckHuntActive(channel)) // Cucks can be spawned one-time for the channel, ignore reproducing them if hunt is not currently running there
            {
                if (LastActivity.ContainsKey(channel))
                {
                    if (LastActivity[channel].LastCuckSpawn > LastActivity[channel].LastMessage && _config.NoSpawnOnNoActivity)
                    {
                        _log.Debug($"Skipping scheduled cuck spawn in {channel} as it is inactive. LastActivity data follows");
                        _log.Debug(JsonConvert.SerializeObject(LastActivity, Formatting.Indented));
                        CreateThread(channel);
                        return;
                    }

                    LastActivity[channel].LastCuckSpawn = DateTime.Now;
                }
                else
                {
                    LastActivity.Add(channel, new ActivityTime
                    {
                        LastCuckSpawn = DateTime.Now,
                        LastMessage = DateTime.MinValue
                    });
                }
                CreateCuck(channel);
            }

            DestroyThread(channel, removeCuck: false);
        }

        public void SetScore(string nick, string channel, int? removals = null, int? deports = null, int? helicopters = null, bool createIfNotExists = true)
        {
            CuckHuntConfigModel.CuckConfig.CuckStat nickStats = GetCuckStat(nick, channel);
            bool existing = true;

            if (nickStats == null)
            {
                if (createIfNotExists)
                {
                    existing = false;
                    nickStats = new CuckHuntConfigModel.CuckConfig.CuckStat
                    {
                        Channel = channel,
                        Nick = nick,
                        GetEmOutCount = 0,
                        KilledCount = 0,
                        HelicopterCount = 0
                    };
                }
                else
                {
                    return;
                }
            }
            if (removals != null)
            {
                nickStats.GetEmOutCount = (int) removals;
            }
            if (deports != null)
            {
                nickStats.KilledCount = (int) deports;
            }
            if (helicopters != null)
            {
                nickStats.HelicopterCount = (int) helicopters;
            }

            if (existing)
            {
                _config.Stats.Remove(GetCuckStat(nick, channel));
            }

            _config.Stats.Add(nickStats);
            SaveConfig();
        }

        public CuckHuntConfigModel.CuckConfig.CuckStat GetCuckStat(string nick, string channel)
        {
            return _config.Stats.FirstOrDefault(x => x.Nick == nick && x.Channel == channel);
        }

        public void ProcessMessage(object sender, IrcEventArgs eventArgs)
        {
            if (eventArgs.Data.Message == null)
            {
                return;
            }
            string nick = eventArgs.Data.From.Split('!')[0];
            string channel = eventArgs.Data.Channel;

            
            if (eventArgs.Data.Message[0] == CommandPrefix && !_config.IgnoreList.Contains(nick))
            {
                string message = eventArgs.Data.Message.TrimStart(CommandPrefix);

                if (message.ToLower().StartsWith("cucks")) // Not ToLowering before this because case does matter for looking up user stats
                {
                    List<string> args = message.Split(' ').ToList();
                    if (args.Count > 1)
                    {
                        string requestedNick = args[1];
                        CuckHuntConfigModel.CuckConfig.CuckStat stat =
                            GetCuckStat(requestedNick, channel);
                        if (stat == null)
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"Requested nick '{requestedNick}' has no stats.");
                        }
                        else
                        {
                            _client.SendMessage(SendType.Message, channel, 
                                $"{stat.GetEmOutCount} cucks removed, {stat.KilledCount} cucks deported and {stat.HelicopterCount} taken for a helicopter ride by {stat.Nick}");
                        }
                    }
                    else
                    {
                        string getOutMessage = "Cucks Removed:";
                        List<CuckHuntConfigModel.CuckConfig.CuckStat> channelStats =
                            (from stat in _config.Stats where stat.Channel == channel select stat).ToList();
                        channelStats.Sort((x, y) => y.GetEmOutCount.CompareTo(x.GetEmOutCount));
                        int i = 0;
                        foreach (CuckHuntConfigModel.CuckConfig.CuckStat stat in channelStats)
                        {
                            if (i == 10) break;
                            getOutMessage += $" {stat.Nick.Insert(stat.Nick.Length / 2, "\u2063").Insert(1, "\u2063")}: {stat.GetEmOutCount};";
                            i++;
                        }
                        i = 0;
                        string deportedMessage = "Cucks Deported:";
                        channelStats.Sort((x, y) => y.KilledCount.CompareTo(x.KilledCount));
                        foreach (CuckHuntConfigModel.CuckConfig.CuckStat stat in channelStats)
                        {
                            if (i == 10) break;
                            deportedMessage += $" {stat.Nick.Insert(stat.Nick.Length / 2, "\u2063").Insert(1, "\u2063")}: {stat.KilledCount};";
                            i++;
                        }
                        i = 0;
                        string helicopterMessage = "Cucks Helicopter'd:";
                        channelStats.Sort((x, y) => y.HelicopterCount.CompareTo(x.HelicopterCount));
                        foreach (CuckHuntConfigModel.CuckConfig.CuckStat stat in channelStats)
                        {
                            if (i == 10) break;
                            helicopterMessage += $" {stat.Nick.Insert(stat.Nick.Length / 2, "\u2063").Insert(1, "\u2063")}: {stat.HelicopterCount};";
                            i++;
                        }
                        _client.SendMessage(SendType.Message, channel, getOutMessage);
                        _client.SendMessage(SendType.Message, channel, deportedMessage);
                        _client.SendMessage(SendType.Message, channel, helicopterMessage);
                        
                    }

                }

                message = message.ToLower(); // Case doesn't matter at this point
                if (message.StartsWith("deport") || message.StartsWith("getout") || message.StartsWith("helicopter"))
                {
                    bool kill = message.StartsWith("deport");
                    bool helicopter = message.StartsWith("helicopter");

                    Cuck currentCuck = GetCuck(channel);

                    if (currentCuck == null && !_config.CuckPresentExempt.Contains(nick))
                    {
                        if (_lastDeportedCuck == null)
                        {
                            _client.SendMessage(SendType.Message, channel, "What? There are no cucks around!");
                        }
                        else
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"What? There are no cucks around! A cuck was last removed {(DateTimeOffset.UtcNow - _lastDeportedCuck).Value.TotalSeconds} seconds ago");
                        }
                        return;
                    }

                    TimeSpan timeElapsed;

                    if (currentCuck != null)
                    {
                        timeElapsed = DateTime.Now - currentCuck.Appeared;
                        RemoveCuck(channel);
                    }
                    else if (_config.CuckPresentExempt.Contains(nick))
                    {
                        timeElapsed = new TimeSpan(1);
                    }
                    else
                    {
                        _backtraceClient.Attributes.Add("Nick", nick);
                        _backtraceClient.Send("currentCuck was null, user was not cuck exempt yet somehow we got this far.");
                        throw new Exception("currentCuck was null, user was not cuck exempt yet somehow we got this far.");
                    }

                    ReloadConfig();
                    CuckHuntConfigModel.CuckConfig.CuckStat cuckStat = GetCuckStat(nick, channel) ??
                                                                       new CuckHuntConfigModel.CuckConfig.CuckStat { Channel = channel, GetEmOutCount = 0, KilledCount = 0, Nick = nick, HelicopterCount = 0 };
                    if (kill)
                    {
                        SetScore(nick, channel, null, cuckStat.KilledCount + 1, createIfNotExists: true);
                        cuckStat = GetCuckStat(nick, channel);

                        if (_config.AssumeMaleGender)
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"{nick} deported the cuck in {(int)timeElapsed.TotalSeconds}.{timeElapsed.Milliseconds} seconds! He has deported {cuckStat.KilledCount} cucks within {cuckStat.Channel}");
                        }
                        else
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"{nick} deported the cuck in {(int)timeElapsed.TotalSeconds}.{timeElapsed.Milliseconds} seconds! They have deported {cuckStat.KilledCount} cucks within {cuckStat.Channel}");
                        }
                        
                    }
                    else if (helicopter)
                    {
                        SetScore(nick, channel, null, helicopters: cuckStat.HelicopterCount + 1, createIfNotExists: true);
                        cuckStat = GetCuckStat(nick, channel);

                        if (_config.AssumeMaleGender)
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"{nick} took the cuck for a helicopter ride in {(int)timeElapsed.TotalSeconds}.{timeElapsed.Milliseconds} seconds! He has helicopter'd {cuckStat.HelicopterCount} cucks within {cuckStat.Channel}");
                        }
                        else
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"{nick} took the cuck for a helicopter ride in {(int)timeElapsed.TotalSeconds}.{timeElapsed.Milliseconds} seconds! They have helicopter'd {cuckStat.HelicopterCount} cucks within {cuckStat.Channel}");
                        }
                    }
                    else
                    {
                        SetScore(nick, channel, cuckStat.GetEmOutCount + 1, createIfNotExists: true);
                        cuckStat = GetCuckStat(nick, channel);

                        if (_config.AssumeMaleGender)
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"{nick} got rid of the cuck in {(int)timeElapsed.TotalSeconds}.{timeElapsed.Milliseconds} seconds! He has gotten rid of {cuckStat.GetEmOutCount} cucks within {cuckStat.Channel}");
                        }
                        else
                        {
                            _client.SendMessage(SendType.Message, channel,
                                $"{nick} got rid of the cuck in {(int)timeElapsed.TotalSeconds}.{timeElapsed.Milliseconds} seconds! They have gotten rid of {cuckStat.GetEmOutCount} cucks within {cuckStat.Channel}");
                        }
                    }

                    if (_config.PunishAdmins)
                    {
                        if (LastAdminWhoSpawnedCuck == nick)
                        {
                            _client.SendMessage(SendType.Message, channel, $"{nick} is cheating scum, 10 points will be removed from his scores.");
                            SetScore(nick, channel, cuckStat.GetEmOutCount - 10, cuckStat.KilledCount - 10, cuckStat.HelicopterCount - 10);
                        }
                        LastAdminWhoSpawnedCuck = string.Empty;
                    }

                    _log.Info($"{nick} removed a cuck in {timeElapsed.TotalMilliseconds} ms ({(int)timeElapsed.TotalSeconds}s) kill={kill}, helicopter={helicopter}. cuckStat.KilledCount={cuckStat.KilledCount}, cuckStat.HelicopterCount={cuckStat.HelicopterCount}, cuckStat.GetEmOutCount={cuckStat.GetEmOutCount}");

                    if (currentCuck != null)
                    {
                        if (!currentCuck.ManuallyCreated)
                        {
                            CreateThread(channel);
                        }
                    }
                    
                    // Early return is necessary so cuckhunt commands don't count as "channel activity"
                    // tfw I fucked it up and the activity feature never worked
                    return;
                }
            }
            
            if (LastActivity.ContainsKey(channel))
            {
                LastActivity[channel].LastMessage = DateTime.Now;
            }
            else
            {
                LastActivity.Add(channel, new ActivityTime
                {
                    LastCuckSpawn = DateTime.MinValue,
                    LastMessage = DateTime.Now
                });
            }
        }

        public void StartHunt(string channel)
        {
            _log.Info($"Starting hunt in {channel}");
            _config.Channels.Add(channel);
            SaveConfig();
            CreateThread(channel);
        }

        public void StopHunt(string channel)
        {
            _log.Info($"Stopping hunt in {channel}");
            _config.Channels.Remove(channel);
            SaveConfig();
            if (IsCuckPresent(channel))
            {
                RemoveCuck(channel);
            }
        }

        public void CreateThread(string channel)
        {
            int sleepTime = _getRandom();
            CancellationTokenSource cts = new CancellationTokenSource();
            Thread newThread = new Thread(() => CuckThreadCallback(channel, sleepTime, cts.Token));
            _threads.Add(new CuckThread { Channel = channel, Thread = newThread, Cts = cts});
            newThread.Start();
        }

        public void DestroyThread(string channel, bool removeCuck = true)
        {
            if (IsCuckPresent(channel) && removeCuck)
            {
                RemoveCuck(channel);
            }

            CuckThread cuckThread = _threads.Find(t => t.Channel == channel);

            if (cuckThread == null) return;
            if (!cuckThread.Thread.IsAlive) return;
            
            cuckThread.Cts.Cancel();
            _threads.Remove(cuckThread);
        }

        public string GetMessage()
        {
            return _config.PhraseList[new Random().Next(0, _config.PhraseList.Count - 1)];
        }

        private int _getRandom()
        {
            return new Random().Next(_config.Random[0] * 1000, _config.Random[1] * 1000);
        }

        public List<Cuck> GetCucks()
        {
            return _cucks;
        }

        public void CreateCuck(string channel, bool manuallyCreated = false)
        {
            _cucks.Add(new Cuck
            {
                Channel = channel,
                Appeared = DateTime.Now,
                ManuallyCreated = manuallyCreated
            });
            _client.SendMessage(SendType.Message, channel, $"A wild cuck has appeared in the channel! They yelled \"{GetMessage()}\"");
            _client.SendMessage(SendType.Message, channel, "!deport them or !getout or !helicopter to GET 'EM OUTTA HERE");

            _log.Debug($"Created cuck for {channel}");
        }

        public void RemoveCuck(string channel)
        {
            foreach (Cuck cuck in _cucks)
            {
                if (cuck.Channel == channel)
                {
                    _cucks.Remove(cuck);
                    _lastDeportedCuck = DateTimeOffset.UtcNow;
                    _log.Debug($"Removed cuck for {channel}");
                    break;
                }
            }
        }

        public Cuck GetCuck(string channel)
        {
            _log.Debug($"Getting cuck for {channel}");
            return _cucks.FirstOrDefault(cuck => cuck.Channel == channel);
        }

        public bool IsCuckPresent(string channel)
        {
            Cuck cuck = GetCuck(channel);
            bool present = cuck != null;
            _log.Debug($"IsCuckPresent({channel}) -> returned {present}");
            return present;
        }

        public bool IsCuckHuntActive(string channel)
        {
            _log.Debug($"Checking if cuckhunt is active in {channel}");
            return _threads.Find(t => t.Channel == channel) != null;
            //return _config.Channels.Contains(channel);
        }

        public class CuckThread
        {
            public string Channel { get; set; }
            public Thread Thread { get; set; }
            public CancellationTokenSource Cts { get; set; }
        }

        public class Cuck
        {
            public string Channel { get; set; }
            public DateTime Appeared { get; set; }
            public bool ManuallyCreated { get; set; }
        }

        public class ActivityTime
        {
            public DateTime LastMessage { get; set; }
            public DateTime LastCuckSpawn { get; set; }
        }
    }
}
