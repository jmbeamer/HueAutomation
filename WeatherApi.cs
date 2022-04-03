using Newtonsoft.Json;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;

namespace HueAutomation
{
    public class WeatherApi
    {
        public WeatherApi(string apiKey, double latitude, double longitude)
        {
            Key = apiKey;
            Latitude = latitude;
            Longitude = longitude;
        }
        public string Key { get; }
        public string Uri { get; } = "https://api.openweathermap.org/data/2.5/weather";
        public double Latitude { get; }
        public double Longitude { get; }

        public async Task<double> GetCurrentTemperature()
        {
            var weather = await GetCurrentWeather();
            var json = JObject.Parse(weather);
            var temperature = double.Parse(json.SelectToken("main.temp").ToString());
            return temperature;
        }
        public async Task<string> GetCurrentWeather()
        {
            var builder = new UriBuilder(Uri);
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(Uri);
            query["lat"] = Latitude.ToString();
            query["lon"] = Longitude.ToString();
            query["units"] = "imperial";
            query["appid"] = Key;
            builder.Query = query.ToString();
            var request = builder.ToString();

            using var client = new HttpClient();
            var response = await client.GetAsync(request);

            // should do more to handle errors
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
    }
}