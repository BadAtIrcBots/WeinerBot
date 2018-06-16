using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Configs;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.NoPrefix]
    [Admin.RequiredRight(AdminConfigModel.Right.Guest)]
    [Admin.DoNotBreakAfterExecution]
    internal class RandomKick : IAdminCommand
    {
        private ILog _log = LogManager.GetLogger(typeof(RandomKick));
        
        public string Name { get; } = "RandomKick";

        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@".+", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            RandomKickConfigModel config =
                ConfigHelpers.LoadConfig<RandomKickConfigModel>(ConfigHelpers.ConfigPaths.RandomKickConfig);

            if (!config.Enabled) return;

            RandomKickConfigModel.RandomKickTarget target =
                config.Targets.FirstOrDefault(t => t.Nick == eventArgs.Data.From.GetNick());

            if (target == null)
            {
                return;
            }

            // Adding the target chance is just so that it doesn't end up with negative numbers
            // I don't know if negatives would be an issue but let's not find out the hard way
            int rangeBase =
                new Random(Guid.NewGuid().GetHashCode()).Next(0 + target.Chance,
                    config.ChanceThreshold + target.Chance);
            int chance =
                new Random(Guid.NewGuid().GetHashCode()).Next(0 + target.Chance,
                    config.ChanceThreshold + target.Chance);
            _log.Debug($"Got range {rangeBase} and chance {chance} for kicking {eventArgs.Data.From.GetNick()} in {eventArgs.Data.Channel}, their configured chance is {target.Chance}");
            _log.Debug($"Range for {chance} to fall in is {rangeBase - (target.Chance / 2)}-{rangeBase + (target.Chance / 2)}");

            if (chance >= rangeBase - (target.Chance / 2) && chance <= rangeBase + (target.Chance / 2))
            {
                string message =
                    config.MessageList[new Random(Guid.NewGuid().GetHashCode()).Next(0, config.MessageList.Count - 1)];
                _log.Debug($"Going to kick {eventArgs.Data.From} now");
                client.RfcKick(eventArgs.Data.Channel, eventArgs.Data.From.GetNick(), message);
            }
        }
    }
}