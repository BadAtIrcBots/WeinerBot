using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(15)]
    internal class AlabamaElectionCommand : ICommand
    {
        public string CommandName { get; } = "AlabamaElectionResults";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("^al$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            if (!File.Exists("Config\\al_election.json"))
            {
                return null;
            }
            AlabamaElectionConfigModel config = new Configs.BaseConfig().LoadConfig<AlabamaElectionConfigModel>("Config\\al_election.json");

            List<PoliticoResultsModel> results =
                JsonConvert.DeserializeObject<List<PoliticoResultsModel>>(Http.GetJson(new Uri(config.PoliticoUri),
                    compression: true, fuzzUserAgent: true));
            DateTime lastUpdated = JsonConvert.DeserializeObject<LastUpdatedModel>(
                Http.GetJson(new Uri(config.LastUpdatedUri), compression: true, fuzzUserAgent: true), new IsoDateTimeConverter { DateTimeFormat = "ddd MMM dd HH:mm:ss EST yyyy"}).LastUpdated;

            var moore = results.Find(d => d.PolId == config.RoyMooreId);
            var jones = results.Find(d => d.PolId == config.DougJonesId);
            var writeIn = results.Find(d => d.PolId == config.WriteInId);

            var c = new Colours();
            var r = IrcConstants.IrcNormal;
            var co = IrcConstants.IrcColor;

            DateTime lastUpdatedUtc = TimeZoneInfo.ConvertTimeToUtc(lastUpdated,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            
            return new List<string>
            {
                $"AL 2017 Senate Results: {co}{c.LightRed}Roy Moore{r}: {moore.VoteCount:n0} votes ({(moore.VotePct):P}) | " +
                $"{co}{c.Blue}Doug Jones{r}: {jones.VoteCount:n0} votes ({(jones.VotePct):P}) | " +
                $"{co}{c.Green}Other{r}: {writeIn.VoteCount:n0} votes ({(writeIn.VotePct):P}) | " +
                $"{(moore.PrecinctsReportingPercentage):P} reporting - Last Updated: {(DateTime.UtcNow - lastUpdatedUtc).TotalMinutes:n2} minutes ago"
            };
        }

        internal class LastUpdatedModel // Last minute addition!
        {
            [JsonProperty("date")]
            public DateTime LastUpdated { get; set; }
        }
    }
}