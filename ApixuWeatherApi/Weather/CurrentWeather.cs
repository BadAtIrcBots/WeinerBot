using System;
using System.Threading.Tasks;
using ApixuWeatherApi.Models;
using Newtonsoft.Json;

namespace ApixuWeatherApi.Weather
{
    public class CurrentWeather
    {
        public static async Task<CurrentWeatherModel.CurrentWeather> GetCurrentWeatherAsync(string query, string apiKey)
        {
            string json;
            try
            {
                json =
                    await Helpers.Http.GetJson(new Uri($"http://api.weatherstack.com/forecast?access_key={apiKey}&query={query}"));
            }
            catch (Helpers.Http.BadRequestException)
            {
                throw new Exceptions.QueryNotFoundException($"{query} not found");
            }

            return JsonConvert.DeserializeObject<CurrentWeatherModel.CurrentWeather>(json);
        }
    }
}
