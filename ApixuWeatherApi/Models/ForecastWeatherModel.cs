using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApixuWeatherApi.Models
{
    public class ForecastWeatherModel
    {
        public class ForecastWeather
        {
            [JsonProperty("location")]
            public CurrentWeatherModel.Location Location { get; set; }
            [JsonProperty("current")]
            public CurrentWeatherModel.CurrentCondition Current { get; set; }
            [JsonProperty("forecast")]
            public Forecast Forecast { get; set; }
        }

        public class Day
        {
            [JsonProperty("maxtemp_c")]
            public double MaxTempCelsius { get; set; }
            [JsonProperty("maxtemp_f")]
            public double MaxTempFahrenheit { get; set; }
            [JsonProperty("mintemp_c")]
            public double MinTempCelsius { get; set; }
            [JsonProperty("mintemp_f")]
            public double MinTempFahrenheit { get; set; }
            [JsonProperty("avgtemp_c")]
            public double AvgTempCelsius { get; set; }
            [JsonProperty("avgtemp_f")]
            public double AvgTempFahrenheit { get; set; }
            [JsonProperty("maxwind_mph")]
            public double MaxWindMph { get; set; }
            [JsonProperty("maxwind_kph")]
            public double MaxWindKph { get; set; }
            [JsonProperty("totalprecip_mm")]
            public double TotalPrecipMm { get; set; }
            [JsonProperty("totalprecip_in")]
            public double TotalPrecipIn { get; set; }
            [JsonProperty("avgvis_km")]
            public double AvgVisibilityKm { get; set; }
            [JsonProperty("avgvis_miles")]
            public double AvgVisibilityMiles { get; set; }
            [JsonProperty("avghumidity")]
            public double AvgHumidity { get; set; }
            [JsonProperty("condition")]
            public CurrentWeatherModel.Condition Condition { get; set; }
        }

        public class Astro
        {
            [JsonProperty("sunrise")]
            public string Sunrise { get; set; }
            [JsonProperty("sunset")]
            public string Sunset { get; set; }
            [JsonProperty("moonrise")]
            public string Moonrise { get; set; }
            [JsonProperty("moonset")]
            public string Moonset { get; set; }
        }

        public class Hour
        {
            [JsonProperty("time_epoch")]
            public long TimeEpoch { get; set; }
            [JsonProperty("time")]
            public DateTime Time { get; set; }
            [JsonProperty("temp_c")]
            public double TempCelsius { get; set; }
            [JsonProperty("temp_f")]
            public double TempFahrenheit { get; set; }
            [JsonProperty("is_day")]
            public bool IsDay { get; set; }
            [JsonProperty("condition")]
            public CurrentWeatherModel.Condition Condition { get; set; }
            [JsonProperty("wind_mph")]
            public double WindMph { get; set; }
            [JsonProperty("wind_kph")]
            public double WindKph { get; set; }
            [JsonProperty("wind_degree")]
            public int WindDegree { get; set; }
            [JsonProperty("wind_dir")]
            public string WindDirection { get; set; }
            [JsonProperty("pressure_mb")]
            public double PressureMb { get; set; }
            [JsonProperty("pressure_in")]
            public double PressureIn { get; set; }
            [JsonProperty("precip_mm")]
            public double PrecipMm { get; set; }
            [JsonProperty("precip_in")]
            public double PrecipIn { get; set; }
            [JsonProperty("humidity")]
            public int Humidity { get; set; }
            [JsonProperty("cloud")]
            public bool Cloud { get; set; }
            [JsonProperty("feelslike_c")]
            public double FeelsLikeCelsius { get; set; }
            [JsonProperty("feelslike_f")]
            public double FeelsLikeFahrenheit { get; set; }
            [JsonProperty("windchill_c")]
            public double WindChillCelsius { get; set; }
            [JsonProperty("windchill_f")]
            public double WindChillFahrenheit { get; set; }
            [JsonProperty("heatindex_c")]
            public double HeatIndexCelsius { get; set; }
            [JsonProperty("heatindex_f")]
            public double HeatIndexFahrenheit { get; set; }
            [JsonProperty("dewpoint_c")]
            public double DewPointCelsius { get; set; }
            [JsonProperty("dewpoint_f")]
            public double DewPointFahrenheit { get; set; }
            [JsonProperty("will_it_rain")]
            public bool WillItRain { get; set; }
            [JsonProperty("will_it_snow")]
            public bool WillItSnow { get; set; }
            [JsonProperty("vis_km")]
            public double VisibilityKm { get; set; }
            [JsonProperty("vis_miles")]
            public double VisibilityMiles { get; set; }
        }

        public class ForecastDay
        {
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("date_epoch")]
            public long DateEpoch { get; set; }
            [JsonProperty("day")]
            public Day Day { get; set; }
            [JsonProperty("astro")]
            public Astro Astro { get; set; }
            [JsonProperty("hour")]
            public List<Hour> Hours { get; set; }
        }

        public class Forecast
        {
            [JsonProperty("forecastday")]
            public List<ForecastDay> ForecastDays { get; set; }
        }
    }
}
