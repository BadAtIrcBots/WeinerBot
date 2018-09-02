using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Meebey.SmartIrc4net;
using SharpRaven;
using SharpRaven.Data;
using TrumpBot.Configs;
using TrumpBot.Extensions;
using TrumpBot.Models.Config;
using TrumpBot.Modules.Commands;
using TrumpBot.Services;

namespace TrumpBot.Modules
{
    public class Command
    {
        private IrcClient _client;
        private ILog _log = LogManager.GetLogger(typeof(Command));
        private IrcBot _ircBot;
        private IEnumerable<ICommand> Commands;
        public char CommandPrefix = '!';
        private CommandConfigModel _config;
        private RavenClient _ravenClient = Raven.GetRavenClient();
        internal List<Thread> Threads = new List<Thread>();

        internal Command(IrcClient client, IrcBot bot)
        {
            LoadConfig();
            _client = client;
            _ircBot = bot;
            Type interfaceType = typeof(ICommand);
            Commands =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(Activator.CreateInstance).Cast<ICommand>().OrderByDescending(c => c.Priority);

            if (Commands == null) throw new Exception("null commands available");
            
            foreach (ICommand command in Commands)
            {
                _log.Info($"Found command: {command.CommandName}");
            }
        }

        internal void LoadConfig()
        {
            _config = ConfigHelpers.LoadConfig<CommandConfigModel>(ConfigHelpers.ConfigPaths.CommandConfig);
            _log.Debug("Config loaded");
        }

        internal List<string> GetCachedMessage(string key)
        {
            return Cache.Get(key) as List<string>;
        }

        internal void CacheMessage(string key, List<string> message, int secondsUntilExpiration)
        {
            Cache.Set(key, message, DateTimeOffset.UtcNow.AddSeconds(secondsUntilExpiration));
        }

        internal void StopAllThreads()
        {
            foreach (Thread thread in Threads)
            {
                thread.Abort();
            }
        }

        internal void CleanupThreads()
        {
            // Thread cleanup
            List<Thread> threadsToRemove = new List<Thread>();
            foreach (Thread thread in Threads)
            {
                if (!thread.IsAlive)
                {
                    threadsToRemove.Add(thread);
                }
            }

            foreach (Thread thread in threadsToRemove)
            {
                Threads.Remove(thread);
            }
        }

        internal void RunCommand(IrcEventArgs eventArgs, ICommand command, Match match)
        {
            string message = eventArgs.Data.Message.TrimStart(CommandPrefix);
            string nick = eventArgs.Data.Nick.Split('!')[0];
            bool cached = false;
            var commandEventParams = eventArgs.Data.CastToIrcChannelMessageEventData();
            commandEventParams.Message = message;

            CleanupThreads();

            DoNotReportException doNotReportException =
                (DoNotReportException) Attribute.GetCustomAttribute(command.GetType(),
                    typeof(DoNotReportException));
            CacheOutput cacheOutput =
                (CacheOutput) Attribute.GetCustomAttribute(command.GetType(), typeof(CacheOutput));

            bool cacheOutputMessage = cacheOutput != null;
            bool reportException = doNotReportException == null;

            try
            {
                _log.Debug($"Handing over to {command}");
                _log.Debug($"cacheOutputMessage = {cacheOutputMessage}");
                List<string> result = null;
                if (cacheOutputMessage && _ircBot.UseCache)
                {
                    _log.Debug($"Looking up {command.CommandName} in cache");
                    result = GetCachedMessage(command.CommandName);
                    _log.Debug($"Got {result}");
                    if (result != null) cached = true;
                }

                if (result == null)
                {
                    result = command.RunCommand(commandEventParams, match.Groups,
                        _ircBot.UseCache);
                }

                if (result != null)
                {
                    if (cacheOutputMessage && _ircBot.UseCache && !cached)
                    {
                        _log.Debug($"Caching {command.CommandName} for {cacheOutput.CacheSeconds}");
                        CacheMessage(command.CommandName, result, cacheOutput.CacheSeconds);
                    }

                    if (_config.ResultSuffix != null)
                    {
                        result[result.Count - 1] = result[result.Count - 1] + _config.ResultSuffix;
                    }

                    foreach (string line in result)
                    {
                        _client.SendMessage(SendType.Message, eventArgs.Data.Channel, line);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Debug("Stacktrace");
                _log.Debug(e.StackTrace);

                if (reportException && e.InnerException == null
                ) // Attribute [DoNotReportException] to suppress this
                {
                    _client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                        $"Well this is embarrassing: {e.Source}: {e.Message}");
                }
                if (e.InnerException != null)
                {
                    _log.Debug(e.InnerException.StackTrace);
                    e.InnerException.Data.Add("RequestorNick", nick);
                    e.InnerException.Data.Add("CommandName", command.CommandName);
                    e.InnerException.Data.Add("Message", message);
                    e.InnerException.Data.Add("NetworkUri", _ircBot.Settings.ConnectionUri);
                    e.InnerException.Data.Add("cacheOutputMessage", cacheOutputMessage);
                    e.InnerException.Data.Add("reportException", reportException);
                    _ravenClient?.Capture(new SentryEvent(e.InnerException));
                    if (reportException)
                    {
                        _client.SendMessage(SendType.Message, eventArgs.Data.Channel,
                            $"Well this is embarassing: {e.InnerException.Source}: {e.InnerException.Message}");
                    }

                    return;
                }
                e.Data.Add("RequestorNick", nick);
                e.Data.Add("CommandName", command.CommandName);
                e.Data.Add("Message", message);
                e.Data.Add("NetworkUri", _ircBot.Settings.ConnectionUri);
                e.Data.Add("cacheOutputMessage", cacheOutputMessage);
                e.Data.Add("reportException", reportException);
                _ravenClient?.Capture(new SentryEvent(e));
            }
        }

        internal void ProcessMessage(object sender, IrcEventArgs eventArgs)
        {
            _log.Debug($"Processing message: {eventArgs.Data.Message}");

            string message = eventArgs.Data.Message.TrimStart(CommandPrefix);
            string nick = eventArgs.Data.Nick.Split('!')[0];
            _log.Debug($"Got message '{message}' and nick '{nick}'");

            if (_config.IgnoreList.Contains(nick))
            {
                _log.Debug($"{nick} was in IgnoreList");
                return;
            }

            if (_config.IgnoreNonVoicedUsers)
            {
                NonRfcChannelUser user = _client.GetChannelUser(eventArgs.Data.Channel, nick) as NonRfcChannelUser;
                if (user == null)
                {
                    _log.Debug(
                        $"Got null when getting {nick} in {eventArgs.Data.Channel}, can't verify if they're non voiced, ignoring.");
                    return;
                }

                if (!user.IsVoice && !user.IsOp && !user.IsHalfop && !user.IsOwner && !user.IsChannelAdmin &&
                    !user.IsIrcOp)
                {
                    _log.Debug($"Ignored {nick} because IgnoreNonVoicedUsers is set to {_config.IgnoreNonVoicedUsers}");
                    return;
                }
            }

            foreach (ICommand command in Commands)
            {
                NoPrefix noPrefix = (NoPrefix) Attribute.GetCustomAttribute(command.GetType(), typeof(NoPrefix));
                UseMainThread useMainThread = (UseMainThread) Attribute.GetCustomAttribute(command.GetType(),
                    typeof(UseMainThread));

                bool runInMainThread = useMainThread != null;
                bool prefixRequired = noPrefix == null;
                bool breakAfterExecution =
                    (BreakAfterExecution) Attribute.GetCustomAttribute(command.GetType(),
                        typeof(BreakAfterExecution)) != null;
                bool matched = false;

                if (prefixRequired)
                {
                    if (eventArgs.Data.Message[0] != _config.Prefix) continue;
                }

                foreach (Regex regex in command.Patterns)
                {
                    Match match = regex.Match(message);
                    if (!match.Success) continue;
                    matched = true;

                    _log.Debug($"Successfully matched message to '{command.CommandName}' with regex '{regex}'");
                    if (runInMainThread)
                    {
                        RunCommand(eventArgs, command, match);
                        continue;
                    }

                    Thread commandThread = new Thread(() => RunCommand(eventArgs, command, match));
                    Threads.Add(commandThread);
                    commandThread.Start();
                }

                if (breakAfterExecution && matched) break;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class NoPrefix : Attribute
        {
            internal bool PrefixRequired;

            internal NoPrefix()
            {
                PrefixRequired = false;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class DoNotReportException : Attribute
        {
            internal bool ReportExceptionToChannel;

            internal DoNotReportException()
            {
                ReportExceptionToChannel = false;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class CacheOutput : Attribute
        {
            internal int CacheSeconds;

            internal CacheOutput(int seconds)
            {
                CacheSeconds = seconds;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class UseMainThread : Attribute
        {
            internal bool UseCommandMainThread;

            internal UseMainThread()
            {
                UseCommandMainThread = true;
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class BreakAfterExecution : Attribute
        {
            internal bool ShouldBreakAfterExecution;

            internal BreakAfterExecution()
            {
                ShouldBreakAfterExecution = true;
            }
        }

        public enum CommandPriority
        {
            Low = 1,
            Normal = 100,
            High = 500,
            VeryHigh = 1000
        }
    }
}