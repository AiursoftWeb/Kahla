using Kahla.Bot.Models;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Kahla.Bot.Bots
{
    public class EchoBot : IBot
    {
        private KahlaUser _botProfile;
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
            var firstReplace = inputMessage
                    .Replace("吗", "")
                    .Replace('？', '！')
                    .Replace('?', '!');
            if (eventContext.Mentioned)
            {
                firstReplace = firstReplace + $" @{eventContext.Message.Sender.NickName.Replace(" ", "")}";
            }
            return firstReplace.Replace($"@{Profile.NickName.Replace(" ", "")}", "");
        }

        public async Task<bool> OnFriendRequest(NewFriendRequestEvent arg)
        {
            await Task.Delay(0);
            return true;
        }
    }
}
