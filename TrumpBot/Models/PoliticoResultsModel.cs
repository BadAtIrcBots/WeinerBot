using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class PoliticoResultsModel
    {
        [JsonProperty("fipscode")]
        public string FipsCode { get; set; }
        [JsonProperty("level")]
        public string Level { get; set; }
        [JsonProperty("polid")]
        public string PolId { get; set; }
        [JsonProperty("precinctsreporting")]
        public int PrecinctsReporting { get; set; }
        [JsonProperty("precinctsreportingpct")]
        public decimal PrecinctsReportingPercentage { get; set; }
        [JsonProperty("precinctstotal")]
        public int PrecinctsTotal { get; set; }
        [JsonProperty("raceid")]
        public string RaceId { get; set; }
        [JsonProperty("statepostal")]
        public string StatePostal { get; set; }
        [JsonProperty("votecount")]
        public int VoteCount { get; set; }
        [JsonProperty("votepct")]
        public decimal VotePct { get; set; }
        [JsonProperty("winner")]
        public bool Winner { get; set; }
    }

    public class PoliticoMidtermsModels
    {
        public class PartyTotalModel
        {
            [JsonProperty("total")]
            public int Total { get; set; }
            [JsonProperty("flips")] // It's null for 'other'
            public int? Flips { get; set; }
        }

        public class RaceModel
        {
            [JsonProperty("dem")]
            public PartyTotalModel Democrats { get; set; }
            [JsonProperty("gop")]
            public PartyTotalModel Republicans { get; set; }
            [JsonProperty("other")]
            public PartyTotalModel Other { get; set; }
            [JsonProperty("undecided")]
            public int Undecided { get; set; }
            // No idea what the below values will look like
            [JsonProperty("projected")]
            public string Projected { get; set; }
            [JsonProperty("call")]
            public string Call { get; set; }
        }

        public class ResultsRootModel
        {
            [JsonProperty("house")]
            public RaceModel House { get; set; }
            [JsonProperty("senate")]
            public RaceModel Senate { get; set; }
        }
    }
}