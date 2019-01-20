using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class AirportModel
    {
        [JsonProperty("icao")]
        public string ICAO { get; set; }
        [JsonProperty("iata")]
        public string IATA { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("elevation")]
        public int Elevation { get; set; }
        [JsonProperty("lat")]
        public decimal Lat { get; set; }
        [JsonProperty("lon")]
        public decimal Lon { get; set; }
        [JsonProperty("tz")]
        public string Timezone { get; set; }
    }
}