using Q42.HueApi;
using Q42.HueApi.Models;
using cc = Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Gamut;
using HueAutomation;

namespace HueAutomation
{
    public class BridgeControl
    {
        public BridgeControl(string ipAddress, string appKey, IEnumerable<string> lightNames)
        {
            BridgeIp = ipAddress;
            AppKey = appKey;
            LightNames = lightNames.ToList();
        }
        private int Max { get; set; } = 100;
        private int Min { get; set; } = 0;
        private string BridgeIp { get; }
        private string AppKey { get; }
        private List<string> LightNames { get; }
        private ILocalHueClient _client;
        private ILocalHueClient Client => _client ?? (_client = new LocalHueClient(BridgeIp,AppKey));

        public async Task SetColorFromTemperature(double temperature)
        {
            var lightIds = await GetLightIds();
            var converter = new ColorConverter();
            var command = new LightCommand();
            
            var color = converter.TemperatureToXyz(temperature);
            command.SetColor(color.x, color.y);
            var result = await Client.SendCommandAsync(command, lightIds);

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")} -- {temperature}");
        }

        public async Task SampleFullSpectrum()
        {
            var lightIds = await GetLightIds();
            var converter = new ColorConverter();
            var command = new LightCommand();

            for (int i = 0; i <= 1000; i++)
            {
                var temp = i / 10.0;
                var color = converter.TemperatureToXyz(temp);
                Console.WriteLine($"{temp}: ({color.x}, {color.y})");
                command.SetColor(color.x, color.y);
                var result = await Client.SendCommandAsync(command, lightIds);

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async Task SamplePoints()
        {
            var lightIds = await GetLightIds();
            var converter = new ColorConverter();
            var command = new LightCommand();

            for (int i = 0; i <= 100; i++)
            {
                var temp = i;
                var color = converter.TemperatureToXyz(temp);
                Console.WriteLine($"{temp}: ({color.x}, {color.y})");
                command.SetColor(color.x, color.y);
                var result = await Client.SendCommandAsync(command, lightIds);
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                if (i % 10 == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        public async Task Compare()
        {
            var lightIds = await GetLightIds();
            var converter = new ColorConverter();

            for (int temp = 0; temp <= 100; temp += 10)
            {
                var rgb = converter.TemperatureToRgb_3Seg(temp);
                var color = converter.RgbToXyz(rgb);
                Console.WriteLine($"{temp}: ({color.x}, {color.y})");

                var command = new LightCommand();
                command.SetColor(color.x, color.y);
                var result = await Client.SendCommandAsync(command, lightIds);

                await Task.Delay(TimeSpan.FromSeconds(5));


                rgb = converter.TemperatureToRgb_4Seg(temp);
                color = converter.RgbToXyz(rgb);
                Console.WriteLine($"{temp}: ({color.x}, {color.y})");

                command = new LightCommand();
                command.SetColor(color.x, color.y);
                result = await Client.SendCommandAsync(command, lightIds);

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        private async Task<List<string>> GetLightIds()
        { 
            var bridge = await Client.GetBridgeAsync();
            var lights = bridge.Lights
                               .Where(l => LightNames.Contains(l.Name))
                               .Select(l => l.Id)
                               .ToList();
            return lights;
        }
        
    }
}
