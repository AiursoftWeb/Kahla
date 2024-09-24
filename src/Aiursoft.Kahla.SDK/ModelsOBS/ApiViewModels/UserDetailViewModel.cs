using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class UserDetailViewModel : AiurResponse
    {
        public required KahlaUser User { get; set; }
        public required bool AreFriends { get; set; }
        public required bool SentRequest { get; set; }
        public Request? PendingRequest { get; set; }
        public int? ConversationId { get; set; }
    }
}
