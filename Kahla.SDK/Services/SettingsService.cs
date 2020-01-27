using Aiursoft.Scanner.Interfaces;
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

        public object this[string key]
        {
            get
            {
                var content = ReadAll();
                if (content.ContainsKey(key.ToLower()))
                {
                    return content[key.ToLower()];
                }
                return null;
            }
            set
            {
                var setting = ReadAll();
                setting[key.ToLower()] = value;
                var settingString = JsonConvert.SerializeObject(setting);
                File.WriteAllText("bot.json", settingString);
            }
        }
    }
}
