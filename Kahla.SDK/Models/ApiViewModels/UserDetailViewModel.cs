using Aiursoft.AiurProtocol;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class UserDetailViewModel : AiurResponse
    {
        public KahlaUser User { get; set; }
        public bool AreFriends { get; set; }
        public bool SentRequest { get; set; }
        public Request PendingRequest { get; set; }
        public int? ConversationId { get; set; }
    }
}
