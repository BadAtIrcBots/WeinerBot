using System;
using Newtonsoft.Json;
using WeinerBot.Services;

namespace WeinerBot.Models
{
    public class CryptoTickerModels
    {
        public class FinexTickerModel
        {
            [JsonProperty("mid")]
            public float Mid { get; set; }
            [JsonProperty("bid")]
            public float Bid { get; set; }
            [JsonProperty("ask")]
            public float Ask { get; set; }
            [JsonProperty("last_price")]
            public float LastPrice { get; set; }
            [JsonProperty("low")]
            public float Low { get; set; }
            [JsonProperty("high")]
            public float High { get; set; }
            [JsonProperty("volume")]
            public float Volume { get; set; }
            [JsonConverter(typeof(JsonConverters.UnixDateTimeFloatConverter))]
            [JsonProperty("timestamp")]
            public DateTime Timestamp { get; set; }
        }
    }
}