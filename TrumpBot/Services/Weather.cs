using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrumpBot.Exceptions;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    public static class Weather
    {
        public static async Task<CurrentWeatherModel.CurrentWeather> GetCurrentWeatherAsync(string query, string apiKey)
        {
            string json;
            try
            {
                json =
                    await Http.GetApixuJson(new Uri($"http://api.weatherstack.com/forecast?access_key={apiKey}&query={query}"));
            }
            catch (Http.BadRequestException)
            {
                throw new QueryNotFoundException($"{query} not found");
            }

            return JsonConvert.DeserializeObject<CurrentWeatherModel.CurrentWeather>(json);
        }
        
        public static async Task<ForecastWeatherModel.ForecastWeather> GetWeatherForecastAsync(string query,
            string apiKey, int days = 1)
        {
            string json;
            try
            {
                json =
                    await Http.GetApixuJson(new Uri(
                        $"http://api.apixu.com/v1/forecast.json?key={apiKey}&q={query}&days={days}"));
            }
            catch (Http.BadRequestException)
            {
                throw new QueryNotFoundException($"{query} not found");
            }

            return JsonConvert.DeserializeObject<ForecastWeatherModel.ForecastWeather>(json);
        }
    }
}