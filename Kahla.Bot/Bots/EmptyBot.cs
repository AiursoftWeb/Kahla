using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using System.Threading.Tasks;

#pragma warning disable CS1998
namespace Kahla.Bot.Bots
{
    public class EmptyBot : BotBase
    {
        public override async Task OnBotInit()
        {

        }

        public override async Task OnFriendRequest(NewFriendRequestEvent arg)
        {

        }

        public override async Task OnMessage(string inputMessage, NewMessageEvent eventContext)
        {

        }
    }
}
#pragma warning restore CS1998
