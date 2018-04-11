using System;
using System.Threading.Tasks;
using ApixuWeatherApi.Models;
using Newtonsoft.Json;

namespace ApixuWeatherApi.Weather
{
    public class ForecastWeather
    {
        public static async Task<ForecastWeatherModel.ForecastWeather> GetWeatherForecastAsync(string query,
            string apiKey, int days = 1)
        {
            string json;
            try
            {
                json =
                    await Helpers.Http.GetJson(new Uri(
                        $"http://api.apixu.com/v1/forecast.json?key={apiKey}&q={query}&days={days}"));
            }
            catch (Helpers.Http.BadRequestException)
            {
                throw new Exceptions.QueryNotFoundException($"{query} not found");
            }

            return JsonConvert.DeserializeObject<ForecastWeatherModel.ForecastWeather>(json);
        }
    }
}
