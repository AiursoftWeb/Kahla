using Kahla.SDK.Events;
using Kahla.SDK.Models;
using System.Threading.Tasks;

namespace Kahla.Bot.Models
{
    public interface IBot
    {
        KahlaUser Profile { set; }
        Task<string> OnMessage(string inputMessage, NewMessageEvent eventContext);
        Task<bool> OnFriendRequest(NewFriendRequestEvent arg);
    }
}
