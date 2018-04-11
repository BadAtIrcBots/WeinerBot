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
}