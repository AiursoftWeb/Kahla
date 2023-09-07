using Aiursoft.Scanner.Abstractions;
using Newtonsoft.Json;

namespace Kahla.SDK.Services
{
    public class SettingsService : ISingletonDependency
    {
        public Dictionary<string, object> Cached;
        public Dictionary<string, object> ReadAll()
        {
            try
            {
                var settingString = File.ReadAllText("bot.json");
                Cached = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingString);
                return Cached;
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
