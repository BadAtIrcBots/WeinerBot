using Newtonsoft.Json;

namespace CoinMarketCapApi.Models
{
    public class TickerModel
    {
        [JsonProperty("id")]
        public string TickerId { get; set; }
        [JsonProperty("name")]
        public string TickerName { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("rank")]
        public string Rank { get; set; } // This should probably be an int but the API is returning it as a string because reasons I suppose
        [JsonProperty("price_usd")]
        public string PriceUsd { get; set; }
        [JsonProperty("price_btc")]
        public string PriceBtc { get; set; }
        [JsonProperty("24h_volume_usd")]
        public string DayVolumeUsd { get; set; }
        [JsonProperty("market_cap_usd")]
        public string MarketCapUsd { get; set; }
        [JsonProperty("available_supply")]
        public string AvailableSupply { get; set; }
        [JsonProperty("total_supply")]
        public string TotalSupply { get; set; }
        [JsonProperty("percent_change_1h")]
        public string PercentChange1Hour { get; set; }
        [JsonProperty("percent_change_24h")]
        public string PercentChange24Hours { get; set; }
        [JsonProperty("percent_change_7d")]
        public string PercentChange7Days { get; set; }
        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }
    }
}
