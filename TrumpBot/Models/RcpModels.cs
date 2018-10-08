using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class RcpModels
    {
        public class Candidate
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("color")]
            public string Color { get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }
            [JsonProperty("status")]
            public string Status { get; set; } // RCP hasn't heard of ints apparently
            [JsonProperty("affiliation")]
            public string Affiliation { get; set; } = string.Empty;
        }

        public class RcpGraphItem
        {
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("candidate")]
            public List<Candidate> Candidates { get; set; }
        }

        public class RcpGraph
        {
            [JsonProperty("rcp_avg")]
            public List<RcpGraphItem> RcpAverage { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; } // Yeah should be an int but it is returned as a string
            [JsonProperty("state")]
            public string State { get; set; }
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("link")]
            public string Link { get; set; }
        }

        public class RcpPollRoot
        {
            [JsonProperty("poll")]
            public RcpGraph Poll { get; set; }
        }
    }
}