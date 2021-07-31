using Newtonsoft.Json;

namespace WeinerBot.Models
{
    public class GdaxTickerApiModel
    {
        [JsonProperty("open")]
        public decimal Open { get; set; }
        [JsonProperty("high")]
        public decimal High { get; set; }
        [JsonProperty("low")]
        public decimal Low { get; set; }
        [JsonProperty("volume")]
        public decimal Volume { get; set; }
        [JsonProperty("last")]
        public decimal Last { get; set; }
        [JsonProperty("volume_30day")]
        public decimal Volume30Day { get; set; }
    }
}