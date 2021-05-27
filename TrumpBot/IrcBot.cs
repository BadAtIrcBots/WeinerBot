using System;
using System.Diagnostics;
using System.Threading;
using Backtrace;
using log4net.Config;
using NLog;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;
using TrumpBot.Modules;
using TrumpBot.Services;

namespace TrumpBot
{
    public class IrcBot
    {
        internal IrcConfigModel.IrcSettings Settings;
        private static IrcClient _ircClient = new();
        internal bool UseCache = true;
        private Logger _log = LogManager.GetCurrentClassLogger();
        public Admin Admin;
        public CuckHunt CuckHunt;
        public Command Command;
        internal TwitterStream TwitterStream;
        private readonly BacktraceClient _backtraceClient;
        public DateTime LastPong = DateTime.UtcNow;
        public Thread PongCheck;

        public IrcBot(IrcConfigModel.IrcSettings settings)
        {
            Settings = settings;
            bool ssl = settings.ConnectionUri.Scheme == "ircs";
            if (Settings.SmartIrc4NetLoggingEnabled)
            {
                BasicConfigurator.Configure();
            }

            Admin = new Admin(_ircClient, this);
            Command = new Command(_ircClient, this);
            CuckHunt = new CuckHunt(_ircClient, this);
            _backtraceClient = Services.Backtrace.GetBacktraceClient();

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
                    new NickServ(_ircClient, Settings).Identify();
                    int i = 0;
                    while (!_ircClient.Usermode.Contains('r') && i < 20)
                    {
                        Thread.Sleep(25);
                        i++;
                    }

                    if (i >= 20)
                    {
                        _log.Error("Reached timeout waiting for usermode +r to be set");
                    }
                }

                foreach (string channel in Settings.AutoJoinChannels)
                {
                    _ircClient.RfcJoin(channel);
                }

                _backtraceClient?.Send("Connected to network successfully");
                LastPong = DateTime.UtcNow;
                if (PongCheck == null || !PongCheck.IsAlive)
                {
                    PongCheck = new Thread(() => CheckConnectionThread());
                    PongCheck.Start();
                }

                if (TwitterStream != null)
                {
                    return;
                }

                // Doubleups will occur if an instance of the object had already been created (old one is not disposed)
                // If this is the first connection these should be null
                TwitterStream = new TwitterStream(_ircClient);

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
            _backtraceClient?.Send("Disconnected from network");
        }

        private void OnPong(object sender, PongEventArgs e)
        {
            _log.Debug($"OnPong fired: Lag = {e.Lag.TotalMilliseconds:N}ms, last pong: {LastPong.ToShortDateString()} {LastPong.ToShortTimeString()} UTC");
            LastPong = DateTime.UtcNow;
        }

        private void CheckConnectionThread()
        {
            _log.Debug("CheckConnectionThread created");
            while (Settings.EnablePongChecking)
            {
                Thread.Sleep(new TimeSpan(0, 0, Settings.PongCheckIntervalMs / 1000));
                _log.Debug("Checking last pong time");
                _log.Debug($"IsConnected => {_ircClient.IsConnected}");
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
                
                _log.Info($"Last pong {(DateTime.UtcNow - LastPong).TotalMilliseconds:N}ms ago, restarting.");
                _log.Info($"Process.GetCurrentProcess().MainModule.FileName -> {Process.GetCurrentProcess().MainModule?.FileName}");
                var process = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true
                };
                Process.Start(process);
                Environment.Exit(1);
            }
        }
    }
}
