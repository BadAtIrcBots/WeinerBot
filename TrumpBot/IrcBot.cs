using System;
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

namespace TrumpBot
{
    public class IrcBot
    {
        internal RedditSticky RedditSticky;
        internal IrcConfigModel.IrcSettings Settings;
        private static IrcClient _ircClient = new IrcClient();
        public const char CommandPrefix = '!';
        internal bool UseCache = true;
        private KickInProgress _kickInProgress = null; // too shit for this world
        private bool EnableJoinProtection = false;
        private MessageInterval _messageInterval;
        private ILog _log = LogManager.GetLogger(typeof(IrcBot));
        public Admin Admin;
        public CuckHunt CuckHunt;
        public Command Command;
        internal TwitterStream TwitterStream;
        private readonly RavenClient _ravenClient;
        internal TetherMonitor TetherMonitor;

        internal List<MessageInterval.Message> Messages =
            JsonConvert.DeserializeObject<List<MessageInterval.Message>>(File.ReadAllText("Config\\messages.json"));

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
            _ircClient.OnQueryNotice += OnQueryNotice;
            _ircClient.OnJoin += OnJoin;
            _ircClient.OnDisconnected += Disconnected;
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

        private void OnJoin(object sender, JoinEventArgs args)
        {
            if (Settings.JoinProtectedChannels.Contains(args.Channel) && EnableJoinProtection)
            {
                _ircClient.SendMessage(SendType.Message, "NICKSERV", $"INFO {args.Who}");
            }
        }

        private void OnQueryNotice(object sender, IrcEventArgs args)
        {
            if (args.Data.From.Split('!')[0].ToLower() == "nickserv")
            {
                Regex notRegistered = new Regex(@"Nick (.+) isn't registered\.", RegexOptions.Compiled);
                Regex registeredNotIdentified = new Regex(@"Last seen .*", RegexOptions.Compiled);
                Regex registeredAndIdentified = new Regex(@"(.+) is currently online\.", RegexOptions.Compiled);
                Regex retrieveNickForNotIdentified = new Regex(@"(.+) is .+", RegexOptions.Compiled);
                if (_kickInProgress == null && retrieveNickForNotIdentified.IsMatch(args.Data.Message))
                {
                    GroupCollection collection = retrieveNickForNotIdentified.Match(args.Data.Message).Groups;
                    string nick = collection[1].Value;
                    _kickInProgress = new KickInProgress
                    {
                        Nick = nick,
                        NickServReply = args.Data.Message + "\r\n"
                    };
                    return;
                }

                if (_kickInProgress == null) return;

                if (notRegistered.IsMatch(_kickInProgress.NickServReply))
                {
                    _kickInProgress = null;
                    return;
                }

                if (registeredAndIdentified.IsMatch(_kickInProgress.NickServReply))
                {
                    _kickInProgress = null;
                    return;
                }

                if (registeredNotIdentified.IsMatch(_kickInProgress.NickServReply))
                {
                    foreach (string channel in _ircClient.JoinedChannels.Cast<string>().Where(channel => Settings.JoinProtectedChannels.Contains(channel)))
                    {
                        _ircClient.RfcKick(channel, _kickInProgress.Nick, $"Please identify for your nick ({_kickInProgress.Nick}) before joining", Priority.Medium);
                    }
                    _kickInProgress = null;
                    return;
                }

                _kickInProgress.NickServReply += $"{args.Data.Message}\r\n";
            }
        }

        private void Connected(object sender, EventArgs eventArgs)
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
            }
            foreach (string channel in Settings.AutoJoinChannels)
            {
                _ircClient.RfcJoin(channel);
            }
            _ravenClient?.AddTrail(new Breadcrumb("Connected") {Message = "Connected to network successfully", Level = BreadcrumbLevel.Info});
            _messageInterval = new MessageInterval(Messages, _ircClient);
            RedditSticky = new RedditSticky(_ircClient);
            if (TwitterStream != null && TetherMonitor != null)
            {
                return;
            }
            // Doubleups will occur if an instance of the object had already been created (old one is not disposed)
            // If this is the first connection these should be null
            TwitterStream = new TwitterStream(_ircClient);
            TetherMonitor = new TetherMonitor(_ircClient);
        }

        private void Disconnected(object sender, EventArgs eventArgs)
        {
            _ravenClient?.AddTrail(new Breadcrumb("Disconnected") {Message = "Disconnected from network", Level = BreadcrumbLevel.Critical});
            if (RedditSticky != null)
            {
                if (RedditSticky.IsAlive())
                {
                    RedditSticky.Stop();
                }
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
                    case "stopmessage":
                        if (Settings.Admins.Contains(nick))
                        {
                            _messageInterval.StopThread(channel);
                            _ircClient.SendMessage(SendType.Notice, nick, $"Any scheduled message intervals for {channel} will be stopped. !rehashmessasges to restart all scheduled messages for all configured channels.");
                        }
                        break;
                    case "rehashmessages":
                        if (Settings.Admins.Contains(nick))
                        {
                            Messages = JsonConvert.DeserializeObject<List<MessageInterval.Message>>(File.ReadAllText("Config\\messages.json"));
                            _messageInterval.RehashMessages(Messages);
                            _ircClient.SendMessage(SendType.Notice, nick, $"Re enabled {Messages.Count} messages");
                        }
                        break;
                    case "fixnick":
                        if (_ircClient.Nickname != Settings.Nick && Settings.Admins.Contains(nick))
                        {
                            _ircClient.RfcNick(Settings.Nick);
                        }
                        break;
                    case "togglejoinprotection":
                        if (Settings.Admins.Contains(nick))
                        {
                            EnableJoinProtection = !EnableJoinProtection;
                            _ircClient.SendMessage(SendType.Message, channel, $"EnableJoinProtection is now {EnableJoinProtection}");
                        }
                        break;
                }
            }
        }
    }

    internal class KickInProgress
    {
        public string Nick { get; set; }
        public string NickServReply { get; set; }
    }
}
