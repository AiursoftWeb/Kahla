using Aiursoft.Pylon.Interfaces;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using System.Threading.Tasks;

namespace Kahla.Bot.Bots
{
    public class EmptyBot : BotBase, ISingletonDependency
    {
        public override KahlaUser Profile { get; set; }

        public override async Task OnInit()
        {

        }

        public override async Task<bool> OnFriendRequest(NewFriendRequestEvent arg)
        {
            return true;
        }

        public override async Task OnMessage(string inputMessage, NewMessageEvent eventContext)
        {

        }
    }
}
