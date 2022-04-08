using System.Web;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Q42.HueApi;
using Q42.HueApi.Models.Groups;

namespace HueAutomation
{
    public class WeatherApi
    {
        public WeatherApi(IConfigurationSection config, BridgeControl bridge)
        {
            Key = config.GetValue<string>("ApiKey");
            Latitude = config.GetValue<double>("Latitude");
            Longitude = config.GetValue<double>("Longitude");
            _lights = config.GetSection("Lights").Get<List<string>>();
        }
        private BridgeControl Bridge { get; }
        public string Key { get; }
        public string Uri { get; } = "https://api.openweathermap.org/data/2.5/weather";
        public double Latitude { get; }
        public double Longitude { get; }
        private List<string> _lights;
        public IReadOnlyList<string> Lights => _lights;

        public async Task<HueResults> SetColorFromTemperature()
        {
            var temp = await GetCurrentTemperature();
            var color = ColorConverter.TemperatureToXyz(temp);

            var command = new LightCommand();
            command.SetColor(color.x, color.y);

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")} -- {temp}");
            return await Bridge.SendCommand(command, Lights);
        }
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