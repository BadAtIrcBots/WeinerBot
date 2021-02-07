using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using SharpRaven;
using SharpRaven.Data;
using TrumpBot.Configs;
using TrumpBot.Models.Config;
using TrumpBot.Modules.AdminCommands;

namespace TrumpBot.Modules
{
    public class Admin
    {
        private IrcClient _client;
        private ILog _log = LogManager.GetLogger(typeof(Admin));
        private IrcBot _ircBot;
        public IEnumerable<object> Commands;

        internal AdminConfigModel.Config Config =
            ConfigHelpers.LoadConfig<AdminConfigModel.Config>(ConfigHelpers.ConfigPaths.AdminConfig);
        internal RavenClient _ravenClient = Services.Raven.GetRavenClient();

        public Admin(IrcClient client, IrcBot bot)
        {
            _client = client;
            _ircBot = bot;
            Type interfaceType = typeof(IAdminCommand);
            Commands =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(Activator.CreateInstance);
            if (Commands == null) throw new Exception("null admin commands available");
            foreach (IAdminCommand command in Commands)
            {
                _log.Info($"Found admin command: {command.Name}");
            }
        }

        public void ReloadAdminConfig(string location = null)
        {
            Config = ConfigHelpers.LoadConfig<AdminConfigModel.Config>(ConfigHelpers.ConfigPaths.AdminConfig);
        }

        public void ProcessMessage(object sender, IrcEventArgs eventArgs)
        {
            _log.Debug($"Processing message: {eventArgs.Data.Message}");
            //if (eventArgs.Data.Message[0] != Config.CommandPrefix) return;

            if (eventArgs.Data.Message == null)
            {
                return;
            }
            string message = eventArgs.Data.Message.TrimStart(Config.CommandPrefix);
            string nick = eventArgs.Data.From.Split('!')[0];
            AdminConfigModel.Right inferredRight = Config.AdminChannels.Contains(eventArgs.Data.Channel) ? AdminConfigModel.Right.Admin : AdminConfigModel.Right.Guest;
            _log.Debug($"Inferred right for {nick}: {inferredRight} (level {(int) inferredRight})");
            AdminConfigModel.User user = Config.Users.FirstOrDefault(configUser => configUser.Nick == nick) ?? new AdminConfigModel.User
            {
                Nick = nick
            };
            _log.Debug($"User instance right for {nick}: {user.Right} (level {(int) user.Right})");
            
            foreach (IAdminCommand command in Commands)
            {
                foreach (Regex regex in command.Patterns)
                {
                    Match match = regex.Match(message);
                    if (!match.Success)
                    {
                        continue;
                    }

                    _log.Debug($"Successfully matched '{match.Value}' with {regex}");

                    RequiredRight requiredRight =
                        (RequiredRight)
                            Attribute.GetCustomAttribute(command.GetType(), typeof(RequiredRight));
                    IgnoreException ignoreException = (IgnoreException) Attribute.GetCustomAttribute(command.GetType(),
                        typeof(IgnoreException));
                    NoPrefix noPrefix = (NoPrefix) Attribute.GetCustomAttribute(command.GetType(), typeof(NoPrefix));
                    DoNotBreakAfterExecution doNotBreakAfterExecution =
                        (DoNotBreakAfterExecution) Attribute.GetCustomAttribute(command.GetType(),
                            typeof(DoNotBreakAfterExecution));

                    if (noPrefix == null)
                    {
                        if (eventArgs.Data.Message[0] != Config.CommandPrefix) continue;
                    }

                    bool shouldIgnoreException = ignoreException != null;

                    if (requiredRight == null)
                    {
                        _log.Debug($"No permissions defined via RequiredRights attribute for command {command.Name}. Not going to allow command to run as this could be a very dangerous mistake.");
                        break;
                    }
                    
                    _log.Debug($"Required right for {command.Name} is {requiredRight.Right} (level {(int) requiredRight.Right})");

                    if (!(user.Right >= requiredRight.Right || inferredRight >= requiredRight.Right))
                    {
                        _log.Info($"{nick} tried to run '{message}' but does not have permissions.");
                        _client.SendMessage(SendType.Notice, nick, $"You do not have access to {command.Name}");
                        break;
                    }

                    _log.Info($"Running {command.Name} for {nick}");
                    try
                    {
                        command.RunCommand(_client, match.Groups, eventArgs, _ircBot);
                    }
                    catch (Exception e)
                    {
                        if (shouldIgnoreException)
                        {
                            throw;
                        }
                        if (e.InnerException == null)
                        {
                            e.Data.Add("RequestorNick", nick);
                            e.Data.Add("AdminCommandName", command.Name);
                            e.Data.Add("Message", message);
                            e.Data.Add("NetworkUri", _ircBot.Settings.ConnectionUri);
                            e.Data.Add("AdminRight", user.Right);
                            _ravenClient?.Capture(new SentryEvent(e));
                            _client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Well this is dumb: {e.Source}: {e.Message}");
                            break;
                        }
                        e.InnerException.Data.Add("RequestorNick", nick);
                        e.InnerException.Data.Add("AdminCommandName", command.Name);
                        e.InnerException.Data.Add("Message", message);
                        e.InnerException.Data.Add("NetworkUri", _ircBot.Settings.ConnectionUri);
                        e.InnerException.Data.Add("AdminRight", user.Right);
                        _ravenClient?.Capture(new SentryEvent(e.InnerException));
                        _client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                            $"Well this is dumb: {e.InnerException.Source}: {e.InnerException.Message}");
                        break;
                    }

                    if (doNotBreakAfterExecution == null) break;
                }
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        public class RequiredRight : Attribute
        {
            public AdminConfigModel.Right Right;

            public RequiredRight(AdminConfigModel.Right right)
            {
                Right = right;
            }
        }

        [AttributeUsage(AttributeTargets
            .All)] // Warning this will crash the bot if the command does raise an exception! (Mostly for the crash command)
        public class IgnoreException : Attribute
        {
            public bool ShouldIgnoreException;

            public IgnoreException()
            {
                ShouldIgnoreException = true;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        public class NoPrefix : Attribute
        {
            public bool ShouldIgnorePrefix;

            public NoPrefix()
            {
                ShouldIgnorePrefix = true;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        public class DoNotBreakAfterExecution : Attribute
        {
            public bool DoNotBreakAfterExec;

            public DoNotBreakAfterExecution()
            {
                DoNotBreakAfterExec = true;
            }
        }
    }
}
