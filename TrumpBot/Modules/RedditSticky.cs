using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using log4net;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using SharpRaven;
using SharpRaven.Data;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules
{
    internal class RedditSticky
    {
        private IrcClient _ircClient;
        private RedditStickyConfigModel _config;
        private List<string> _checkedThings;
        private Reddit _reddit;
        private Thread _thread;
        private ILog _log = LogManager.GetLogger(typeof(RedditSticky));
        private RavenClient _ravenClient = Services.Raven.GetRavenClient();

        internal RedditSticky(IrcClient client)
        {
            _ircClient = client;
            LoadConfig();
            _checkedThings = LoadStoredThings();
            _reddit = new Reddit();
            if (!_config.Enabled)
            {
                // Sick of getting spammed e-mails
                //_ravenClient?.Capture(new SentryEvent("RedditSticky not enabled") {Level = ErrorLevel.Info});
                _log.Info("RedditSticky not enabled");
                return;
            }
            _log.Debug("Creating RedditSticky thread");
            CreateThread();
        }

        internal void LoadConfig()
        {
            _config = (RedditStickyConfigModel) new RedditStickyConfig().LoadConfig();
        }

        internal void Start()
        {
            if (!_thread.IsAlive)
            {
                CreateThread();
            }
        }

        internal void Stop()
        {
            if (_thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        internal bool IsAlive()
        {
            return _thread.IsAlive;
        }

        private void CreateThread()
        {
            Thread newThread = new Thread(() => CheckSubreddit());
            _thread = newThread;
            newThread.Start();
            _log.Debug("RedditSticky thread created");
        }

        private List<string> LoadStoredThings()
        {
            return JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(_config.ThingConfigLocation));
        }

        private void SaveStoredThnigs(List<string> things)
        {
            File.WriteAllText(_config.ThingConfigLocation, JsonConvert.SerializeObject(things));
        }

        private void CheckSubreddit()
        {
            while (true)
            {
                Thread.Sleep(_config.CheckInterval * 1000);
                RedditModel.SubredditModel.Subreddit subreddit;
                string target = _config.Subreddit;
                _log.Debug($"Checking subreddit, targeting {target}");

                try
                {
                    subreddit = _reddit.GetSubreddit(target, false);
                }
                catch (Http.HttpException e)
                {
                    _log.Debug($"Got HTTP exception: {e.Message}");
                    _ravenClient?.Capture(new SentryEvent(e));
                    continue;
                }
                catch (Exception e)
                {
                    _log.Debug($"Got some other excpetion when attepmting to hit the Reddit API: {e.Message}\r\nStacktrace follows");
                    _log.Debug(e);
                    _ravenClient?.Capture(new SentryEvent(e));
                    continue;
                }
                
                foreach (RedditModel.SubredditModel.SubredditChildren child in subreddit.Data.Children)
                {
                    if (child.Thread.IsSticky)
                    {
                        if (!_checkedThings.Contains(child.Thread.Name))
                        {
                            _log.Debug($"Found sticky to broadcast to channels, ID is: {child.Thread.Name}");
                            foreach (string channel in _config.Channels)
                            {
                                if (_ircClient.JoinedChannels.Contains(channel))
                                {
                                    string message = $"{WebUtility.HtmlDecode(child.Thread.Title)} ({child.Thread.Domain}) by {child.Thread.Author}";
                                    if (child.Thread.AuthorFlairText != null)
                                        message += $" ({child.Thread.AuthorFlairText})";
                                    message +=
                                        $", link: {WebUtility.UrlDecode(child.Thread.Url.AbsoluteUri).Replace("&amp;", "&")}";
                                    _ircClient.SendMessage(SendType.Message, channel, message);
                                }
                            }
                            _checkedThings.Add(child.Thread.Name);
                            SaveStoredThnigs(_checkedThings);
                        }
                    }
                }
            }

        }
    }
}
