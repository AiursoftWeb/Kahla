using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Kahla.EchoBot.Bot
{
    public class EchoBotCore
    {
        public async Task SetProfile(KahlaUser user)
        {
            await Task.Delay(400);
            var profilestring = JsonConvert.SerializeObject(user, Formatting.Indented);
            Console.WriteLine(profilestring);
        }

        public async Task<string> ResponseUserMessage(string inputMessage, NewMessageEvent eventContext)
        {
            await Task.Delay(0);
            if (eventContext.Muted)
            {
                return string.Empty;
            }
            var firstReplace = inputMessage
                    .Replace("吗", "")
                    .Replace('？', '！')
                    .Replace('?', '!');
            if (inputMessage.Contains("?") || inputMessage.Contains("？"))
            {
                firstReplace = firstReplace.Replace("是", "又是");
            }
            else
            {
                firstReplace = firstReplace.Replace("是", "也是");
            }
            return firstReplace;
        }

        public async Task<bool> ResponseFriendRequest(NewFriendRequestEvent arg)
        {
            await Task.Delay(0);
            return true;
        }
    }
}
