using Microsoft.Extensions.Configuration;

namespace HueAutomation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var bridgeIp = config.GetValue<string>("BridgeIp");
            var appKey = config.GetValue<string>("AppKey");
            var weatherApiKey = config.GetValue<string>("WeatherApiKey");
            var lights = config.GetSection("Lights").Get<List<string>>();

            var latitude = config.GetValue<double>("Latitude");
            var longitude = config.GetValue<double>("Latitude");

            var bridge = new BridgeControl(bridgeIp, appKey, lights);
            var weather = new WeatherApi(weatherApiKey, latitude, longitude);

            while (true)
            {
                try 
                {
                    double temp = await weather.GetCurrentTemperature();
                    await bridge.SetColorFromTemperature(temp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }
    }
}