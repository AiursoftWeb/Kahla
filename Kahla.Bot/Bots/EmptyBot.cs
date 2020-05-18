using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models.ApiViewModels;
using System.Threading.Tasks;

#pragma warning disable CS1998
namespace Kahla.Bot.Bots
{
    public class EmptyBot : BotBase
    {
        public override async Task OnBotStarted()
        {

        }

        public override async Task OnFriendRequest(NewFriendRequestEvent arg)
        {

        }

        public override async Task OnGroupInvitation(int groupId, NewMessageEvent eventContext)
        {

        }

        public async override Task OnGroupConnected(SearchedGroup group)
        {

        }

        public override async Task OnMessage(string inputMessage, NewMessageEvent eventContext)
        {

        }
    }
}
#pragma warning restore CS1998
