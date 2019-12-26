using Aiursoft.Pylon.Interfaces;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System.IO;

namespace Kahla.SDK.Services
{
    public class SettingsService : ISingletonDependency
    {
        private BotSettings _cached;
        public BotSettings Read()
        {
            try
            {
                var settingString = File.ReadAllText("bot.json");
                _cached = JsonConvert.DeserializeObject<BotSettings>(settingString);
                return _cached;
            }
            catch (IOException)
            {
                return new BotSettings();
            }
        }

        public void Save(string server)
        {
            var setting = Read();
            setting.ServerAddress = server;
            var settingString = JsonConvert.SerializeObject(setting);
            File.WriteAllText("bot.json", settingString);
        }

        public void Save(int coreIndex)
        {
            var setting = Read();
            setting.BotCoreIndex = coreIndex;
            var settingString = JsonConvert.SerializeObject(setting);
            File.WriteAllText("bot.json", settingString);
        }
    }
}
