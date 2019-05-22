﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using log4net.Config;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using SharpRaven;
using SharpRaven.Data;
using TrumpBot.Models.Config;
using TrumpBot.Modules;
using TrumpBot.Services;

namespace TrumpBot
{
    public class IrcBot
    {
        internal RedditSticky RedditSticky;
        internal IrcConfigModel.IrcSettings Settings;
        private static IrcClient _ircClient = new IrcClient();
        public const char CommandPrefix = '!';
        internal bool UseCache = true;
        private ILog _log = LogManager.GetLogger(typeof(IrcBot));
        public Admin Admin;
        public CuckHunt CuckHunt;
        public Command Command;
        internal TwitterStream TwitterStream;
        private readonly RavenClient _ravenClient;
        internal TetherMonitor TetherMonitor;
        public DateTime LastPong = DateTime.UtcNow;
        public Thread PongCheck;

        public IrcBot(IrcConfigModel.IrcSettings settings)
        {
            BasicConfigurator.Configure();
            Settings = settings;
            bool ssl = settings.ConnectionUri.Scheme == "ircs";

            Admin = new Admin(_ircClient, this);
            Command = new Command(_ircClient, this);
            CuckHunt = new CuckHunt(_ircClient, this);
            _ravenClient = Services.Raven.GetRavenClient();

            _ircClient.OnChannelMessage += MessageReceived;
            _ircClient.OnChannelMessage += Admin.ProcessMessage;
            _ircClient.OnChannelMessage += Command.ProcessMessage;
            _ircClient.OnChannelMessage += CuckHunt.ProcessMessage;
            _ircClient.OnConnected += Connected;
            _ircClient.OnDisconnected += Disconnected;
            _ircClient.OnQueryMessage += Command.PrivateMessage;
            _ircClient.OnPong += OnPong;
            _ircClient.AutoNickHandling = true;
            _ircClient.AutoRejoinOnKick = true;
            _ircClient.AutoReconnect = true;
            _ircClient.AutoRelogin = true;
            _ircClient.AutoRejoin = true;
            _ircClient.SupportNonRfc = true;
            _ircClient.ActiveChannelSyncing = true;
            _ircClient.UseSsl = ssl;
            _ircClient.Encoding = System.Text.Encoding.UTF8;
            _ircClient.CtcpVersion = "TrumpBot/1.0";

            _ircClient.Connect(settings.ConnectionUri.IdnHost, settings.ConnectionUri.Port);
            _ircClient.Login(Settings.Nick, Settings.RealName, 0, settings.Username);
            _ircClient.Listen();
        }

        private void Connected(object sender, EventArgs eventArgs)
        {
            try
            {
                if (Settings.NickservPassword != null)
                {
                    new Services.NickServ(_ircClient, Settings).Identify();
                    int i = 0;
                    while (!_ircClient.Usermode.Contains('r') && i < 20)
                    {
                        Thread.Sleep(25);
                        i++;
                    }

                    if (i >= 20)
                    {
                        _log.Debug("Reached timeout waiting for usermode +r to be set");
                    }
                }

                foreach (string channel in Settings.AutoJoinChannels)
                {
                    _ircClient.RfcJoin(channel);
                }

                _ravenClient?.AddTrail(new Breadcrumb("Connected")
                    {Message = "Connected to network successfully", Level = BreadcrumbLevel.Info});
                LastPong = DateTime.UtcNow;
                if (PongCheck == null || !PongCheck.IsAlive)
                {
                    PongCheck = new Thread(() => CheckConnectionThread());
                    PongCheck.Start();
                }

                RedditSticky = new RedditSticky(_ircClient, this);
                if (TwitterStream != null && TetherMonitor != null)
                {
                    return;
                }

                // Doubleups will occur if an instance of the object had already been created (old one is not disposed)
                // If this is the first connection these should be null
                TwitterStream = new TwitterStream(_ircClient);
                TetherMonitor = new TetherMonitor(_ircClient);

            }
            catch (Exception e)
            {
                _log.Error(e);
                if (e.InnerException != null)
                {
                    _log.Error(e.InnerException);
                }

                throw;
            }

        }

        private void Disconnected(object sender, EventArgs eventArgs)
        {
            _ravenClient?.AddTrail(new Breadcrumb("Disconnected") {Message = "Disconnected from network", Level = BreadcrumbLevel.Critical});
            try
            {
                if (RedditSticky != null)
                {
                    if (RedditSticky.IsAlive())
                    {
                        RedditSticky?.Stop();
                    }
                }
            }
            catch (NullReferenceException)
            {
                _ravenClient?.Capture(new SentryEvent(
                    "Against all odds I still got a NullReferenceException when checking for the RedditSticky thread."));
            }
        }

        private void MessageReceived(object sender, IrcEventArgs eventArgs)
        {
            string channel = eventArgs.Data.Channel;
            string message = eventArgs.Data.Message;
            if (message == null) return;
            string nick = eventArgs.Data.From.Split('!')[0];

            if (message[0] == CommandPrefix)
            {
                string cleanedMessage = message.TrimStart(CommandPrefix);
                switch (cleanedMessage.ToLower())
                {
                    case "fixnick":
                        if (_ircClient.Nickname != Settings.Nick && Settings.Admins.Contains(nick))
                        {
                            _ircClient.RfcNick(Settings.Nick);
                        }
                        break;
                }
            }
        }

        private void OnPong(object sender, PongEventArgs e)
        {
            Log.LogToFile($"OnPong fired: Lag = {e.Lag.TotalMilliseconds:N}ms, last pong: {LastPong.ToShortDateString()} {LastPong.ToShortTimeString()} UTC", "pong.log");
            LastPong = DateTime.UtcNow;
        }

        private void CheckConnectionThread()
        {
            Log.LogToFile("CheckConnectionThread created", "pong.log");
            while (Settings.EnablePongChecking)
            {
                Thread.Sleep(new TimeSpan(0, 0, Settings.PongCheckIntervalMs / 1000));
                Log.LogToFile("Checking last pong time", "pong.log");
                Log.LogToFile($"IsConnected => {_ircClient.IsConnected}", "pong.log");
                TimeSpan timeout;
                if (!Settings.TurboPongTimeoutOnDisconnect || _ircClient.IsConnected)
                {
                    timeout = new TimeSpan(0, 0, Settings.PongTimeoutMs / 1000);
                }
                else // If smartirc4net is showing we're not connected and the feature is enabled, then speed up the timeout by 10x (reduces to 1 minute with default settings)
                {
                    timeout = new TimeSpan(0, 0, Settings.PongTimeoutMs / 10000);
                }
                if (DateTime.UtcNow - LastPong <= timeout) continue;
                
                Log.LogToFile($"Last pong {(DateTime.UtcNow - LastPong).TotalMilliseconds:N}ms ago, restarting.", "pong.log");
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location,
                    string.Join(" ", Environment.GetCommandLineArgs()));
                Environment.Exit(1);
            }
        }
    }
}
