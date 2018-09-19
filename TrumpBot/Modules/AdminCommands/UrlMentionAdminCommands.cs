using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using TrumpBot.Models.Config;
using TrumpBot.Services;

namespace TrumpBot.Modules.AdminCommands
{
    public class UrlMentionAdminCommands
    {
        [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
        internal class UrlMentionReloadConfigAdminCommand : IAdminCommand
        {
            public string Name { get; } = "UrlMentionReloadConfig";
            public List<Regex> Patterns { get; } = new List<Regex>
            {
                new Regex(@"^mention reload$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"^url reload$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };
            public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
            {
                Cache.Set(UrlHistory.CacheName, UrlHistory.GetConfig(false), DateTimeOffset.Now.AddMinutes(UrlHistory.CacheExpiration));
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Reloaded URL mention config and saved to cache");
            }
        }
    }
}