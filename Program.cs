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

            var lights = new List<string>(){
                "Couch Lamp"
            };

            var bridge = new BridgeControl(config.GetSection("Bridge"));
            await bridge.Pulsate(lights);
        }
        private async Task TemperatureTracker(IConfiguration config, BridgeControl bridge)
        {
            var weather = new WeatherApi(config.GetSection("Weather"), bridge);

            int startup = 0;
            while (true)
            {
                try 
                {
                    var result = await weather.SetColorFromTemperature();
                    if (result.HasErrors())
                    {
                        // some seriously robust logging here
                        Console.WriteLine(result.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    if (startup < 5)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                        startup++;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                }
            }
        }
    }
}