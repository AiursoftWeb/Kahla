using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Abstract;
using Kahla.Bot.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot.Bots
{
    public class TranslateBot : BotBase, ISingletonDependency
    {
        private KahlaUser _botProfile;
        private readonly BingTranslator _bingTranslator;

        public TranslateBot(BingTranslator bingTranslator)
        {
            _bingTranslator = bingTranslator;
        }

        public override KahlaUser Profile
        {
            get => _botProfile;
            set
            {
                _botProfile = value;
                var profilestring = JsonConvert.SerializeObject(value, Formatting.Indented);
                Console.WriteLine(profilestring);

                BotLogger.LogWarning("Please enter your bing API key:");
                var key = Console.ReadLine();
                _bingTranslator.Init(key);
            }
        }

        public override async Task<string> OnMessage(string inputMessage, NewMessageEvent eventContext)
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
            inputMessage = inputMessage.Replace($"@{Profile.NickName.Replace(" ", "")}", "");
            var translated = _bingTranslator.CallTranslate(inputMessage, "en");
            return translated;
        }

        public override async Task<bool> OnFriendRequest(NewFriendRequestEvent arg)
        {
            await Task.Delay(0);
            return true;
        }
    }
}
