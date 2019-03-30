using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class UserDetailViewModel : AiurProtocol
    {
        public KahlaUser User { get; set; }
        public bool AreFriends { get; set; }
        public bool SentRequest { get; set; }
        public int? ConversationId { get; set; }
    }
}
