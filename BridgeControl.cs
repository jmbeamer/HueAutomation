using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Groups;
using Microsoft.Extensions.Configuration;

namespace HueAutomation
{
    public class BridgeControl
    {
        public BridgeControl(IConfigurationSection config)
        {
            BridgeIp = config.GetValue<string>("IP");
            AppKey = config.GetValue<string>("AppKey");
            Client = new LocalHueClient(BridgeIp,AppKey);
        }
        private Dictionary<string,Light> _lights;
        private int Max { get; set; } = 100;
        private int Min { get; set; } = 0;
        private string BridgeIp { get; }
        private string AppKey { get; }
        private ILocalHueClient Client { get; }
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public async Task<HueResults> SendCommand(LightCommand command, IEnumerable<string> lightNames)
        {
            try 
            {
                await _semaphore.WaitAsync();
                var ids = await GetLightIds(lightNames);
                return await Client.SendCommandAsync(command, ids);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task Pulsate(IEnumerable<string> lightNames) // (double r, double g, double b)
        {
            try 
            {
                await _semaphore.WaitAsync();
                var color = ColorConverter.RgbToXyz((0.0, 48.0, 135.0));

                TimeSpan transition = TimeSpan.FromSeconds(2);
                int repititions = 5;
                var ids = await GetLightIds(lightNames);
                var command = new LightCommand();

                for (int i = 0; i < repititions; i++)
                {
                    command.BrightnessIncrement = 150;
                    command.TransitionTime = transition;
                    command.SetColor(color.x,color.y);
                    await Client.SendCommandAsync(command, ids);
                    await Task.Delay(transition);

                    command.BrightnessIncrement = -150;
                    command.TransitionTime = transition;
                    await Client.SendCommandAsync(command, ids);
                    await Task.Delay(transition);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private async Task<List<string>> GetLightIds(IEnumerable<string> lightNames)
        { 
            if (_lights == null)
            {
                var bridge = await Client.GetBridgeAsync();
                _lights = bridge.Lights.ToDictionary(l => l.Name, l => l);
            }
            var ids = new List<string>();
            foreach (var name in lightNames)
            {
                if (_lights.TryGetValue(name, out Light light))
                {
                    ids.Add(light.Id);
                }
            }
            return ids;
        }
    }
}
