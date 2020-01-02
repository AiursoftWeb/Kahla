using Aiursoft.XelNaga.Interfaces;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Kahla.SDK.Services
{
    public class SettingsService : ISingletonDependency
    {
        public Dictionary<string, object> _cached;
        public Dictionary<string, object> ReadAll()
        {
            try
            {
                var settingString = File.ReadAllText("bot.json");
                _cached = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingString);
                return _cached;
            }
            catch (IOException)
            {
                return new Dictionary<string, object>();
            }
        }

        public object Read(string key)
        {
            var content = ReadAll();
            if (content.ContainsKey(key.ToLower()))
            {
                return content[key.ToLower()];
            }
            return null;
        }

        public void Save(string key, object value)
        {
            var setting = ReadAll();
            setting[key.ToLower()] = value;
            var settingString = JsonConvert.SerializeObject(setting);
            File.WriteAllText("bot.json", settingString);
        }
    }
}
