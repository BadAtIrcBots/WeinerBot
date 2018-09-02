using System;
using Newtonsoft.Json;

namespace ApixuWeatherApi.Models
{
    public class CurrentWeatherModel
    {
        public class Location
        {
            [JsonProperty]
            public string Name { get; set; }
            [JsonProperty("region")]
            public string Region { get; set; }
            [JsonProperty("country")]
            public string Country { get; set; }
            [JsonProperty("lat")]
            public double Lattitude { get; set; } // Not sure I would've used a double but the official API library actually uses one and *presumably* they have tested it!
            [JsonProperty("lon")]
            public double Longitude { get; set; }
            [JsonProperty("tz_id")]
            public string TimeZoneId { get; set; }
            [JsonProperty("localtime_epoch")]
            public long LocalTimeEpoch { get; set; }
            [JsonProperty("localtime")]
            public DateTime LocalTime { get; set; }
        }

        public class Condition
        {
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("icon")]
            public string Icon { get; set; }
            [JsonProperty("code")]
            public int Code { get; set; }
        }

        public class CurrentCondition
        {
            [JsonProperty("last_updated_epoch")]
            public long LastUpdatedEpoch { get; set; }
            [JsonProperty("last_updated")]
            public DateTime LastUpdated { get; set; }
            [JsonProperty("temp_c")]
            public double TemperatureCelsius { get; set; }
            [JsonProperty("temp_f")]
            public double TemperatureFahrenheit { get; set; }
            [JsonProperty("is_day")]
            public bool IsDay { get; set; }
            [JsonProperty("condition")]
            public Condition Condition { get; set; }
            [JsonProperty("wind_mph")]
            public double WindMph { get; set; }
            [JsonProperty("wind_kph")]
            public double WindKph { get; set; }
            [JsonProperty("pressure_mb")]
            public double PressureMillibars { get; set; }
            [JsonProperty("pressure_in")]
            public double PressureInches { get; set; }
            [JsonProperty("precip_mm")]
            public double PrecipitationMillimetres { get; set; }
            [JsonProperty("precip_in")]
            public double PrecipitationInches { get; set; }
            [JsonProperty("humidity")]
            public int Humidity { get; set; } // %
            [JsonProperty("cloud")]
            public bool Cloud { get; set; }
            [JsonProperty("feelslike_c")]
            public double FeelsLikeCelsius { get; set; }
            [JsonProperty("feelslike_f")]
            public double FeelsLikeFahrenheit { get; set; }
            [JsonProperty("vis_km")]
            public double VisibilityKilometres { get; set; }
            [JsonProperty("vis_miles")]
            public double VisibilityMiles { get; set; }
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
