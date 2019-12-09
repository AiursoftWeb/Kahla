using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Services;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot.Bots
{
    public class TranslateBot : BotBase, ISingletonDependency
    {
        private readonly BingTranslator _bingTranslator;
        public override KahlaUser Profile { get; set; }

        public TranslateBot(BingTranslator bingTranslator)
        {
            _bingTranslator = bingTranslator;
        }

        public override Task OnInit()
        {
            var profilestring = JsonConvert.SerializeObject(Profile, Formatting.Indented);
            Console.WriteLine(profilestring);

            BotLogger.LogWarning("Please enter your bing API key:");
            var key = Console.ReadLine();
            _bingTranslator.Init(key);
            return Task.CompletedTask;
        }

        public override async Task OnMessage(string inputMessage, NewMessageEvent eventContext)
        {
            if (eventContext.Muted)
            {
                return;
            }
            if (eventContext.Message.SenderId == Profile.Id)
            {
                return;
            }
            inputMessage = inputMessage.Replace($"@{Profile.NickName.Replace(" ", "")}", "");
            var translated = _bingTranslator.CallTranslate(inputMessage, "en");
            await SendMessage(translated, eventContext.Message.ConversationId, eventContext.AESKey);
        }

        public override async Task<bool> OnFriendRequest(NewFriendRequestEvent arg)
        {
            await Task.Delay(0);
            return true;
        }
    }
}
