using Kahla.Bot.Models;
using Kahla.Bot.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot.Bots
{
    public class TranslateBot : IBot
    {
        private KahlaUser _botProfile;
        private readonly BingTranslator _bingTranslator;
        private readonly BotLogger _botLogger;

        public TranslateBot(
            BingTranslator bingTranslator,
            BotLogger botLogger)
        {
            _bingTranslator = bingTranslator;
            _botLogger = botLogger;
            _botLogger.LogWarning("Please enter your bing API key:");
            var key = Console.ReadLine();
            _bingTranslator.Init(key);
        }

        public KahlaUser Profile
        {
            private get => _botProfile;
            set
            {
                _botProfile = value;
                var profilestring = JsonConvert.SerializeObject(value, Formatting.Indented);
                Console.WriteLine(profilestring);
            }
        }

        public async Task<string> OnMessage(string inputMessage, NewMessageEvent eventContext)
        {
            await Task.Delay(0);
            if (eventContext.Muted)
            {
                return string.Empty;
            }
            if (eventContext.Message.SenderId == Profile.Id)
            {
                return string.Empty;
            }
            var translated = _bingTranslator.CallTranslate(inputMessage, "en");
            return translated;
        }

        public async Task<bool> OnFriendRequest(NewFriendRequestEvent arg)
        {
            await Task.Delay(0);
            return true;
        }
    }
}
