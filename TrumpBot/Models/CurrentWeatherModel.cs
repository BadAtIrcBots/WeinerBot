using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApixuWeatherApi.Models
{
    public class CurrentWeatherModel
    {
        public class Location
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("region")]
            public string Region { get; set; }
            [JsonProperty("country")]
            public string Country { get; set; }
            [JsonProperty("lat")]
            public string Lattitude { get; set; }
            [JsonProperty("lon")]
            public string Longitude { get; set; }
            [JsonProperty("timezone_id")]
            public string TimeZoneId { get; set; }
            [JsonProperty("localtime_epoch")]
            public long LocalTimeEpoch { get; set; }
            [JsonProperty("localtime")]
            public DateTime LocalTime { get; set; }
            [JsonProperty("utc_offset")]
            public string UtcOffset { get; set; }
        }

        public class CurrentCondition
        {
            [JsonProperty("observation_time")]
            public string ObservationTime { get; set; } // Looks like this is UTC now
            [JsonProperty("temperature")]
            public int Temperature { get; set; } // Metric units
            [JsonProperty("weather_code")]
            public int WeatherCode { get; set; }
            [JsonProperty("weather_icons")]
            public List<string> WeatherIcons { get; set; }
            [JsonProperty("weather_descriptions")]
            public List<string> WeatherDescriptions { get; set; }
            [JsonProperty("wind_speed")]
            public int WindSpeed { get; set; }
            [JsonProperty("wind_degree")]
            public int WindDegree { get; set; }
            [JsonProperty("wind_dir")]
            public string WindDirection { get; set; }
            [JsonProperty("pressure")]
            public int Pressure { get; set; }
            [JsonProperty("precip")]
            public float Precipitation { get; set; }
            [JsonProperty("humidity")]
            public int Humidity { get; set; }
            [JsonProperty("cloudcover")]
            public int CloudCover { get; set; }
            [JsonProperty("feelslike")]
            public int FeelsLike { get; set; }
            [JsonProperty("uv_index")]
            public int UvIndex { get; set; }
            [JsonProperty("visibility")]
            public int Visibility { get; set; }
            [JsonProperty("is_day")]
            public string IsDay { get; set; } // wat is a bool amirite ("no", "yes")
        }

        public class CurrentWeather
        {
            [JsonProperty("location")]
            public Location Location { get; set; }
            [JsonProperty("current")]
            public CurrentCondition Current { get; set; }
        }
    }
}
