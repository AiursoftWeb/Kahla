using Aiursoft.XelNaga.Models;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class UserDetailViewModel : AiurProtocol
    {
        public KahlaUser User { get; set; }
        public bool AreFriends { get; set; }
        public bool SentRequest { get; set; }
        public Request PendingRequest { get; set; }
        public int? ConversationId { get; set; }
    }
}
