using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Services;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.ApiAddressModels;
using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Kahla.SDK.Services
{
    public class ConversationService : IScopedDependency
    {
        private readonly AiurProtocolClient _http;
        private readonly KahlaLocation _kahlaLocation;

        public ConversationService(
            AiurProtocolClient http,
            KahlaLocation kahlaLocation)
        {
            _http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<AiurCollection<ContactInfo>> AllAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Conversation", "All", new { });
            return await _http.Get<AiurCollection<ContactInfo>>(url);
        }

        public async Task<AiurCollection<Message>> GetMessagesAsync(int id, int take, string skipFrom)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Conversation", "GetMessage", new
            {
                Id = id,
                Take = take,
                SkipFrom = skipFrom
            });
            var result = await _http.Get<AiurCollection<Message>>(url);
            return result;
        }

        public async Task<AiurValue<Message>> SendMessageAsync(string content, int conversationId)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Conversation", "SendMessage", new { });
            var form = new AiurApiPayload(new SendMessageAddressModel
            {
                Content = content,
                Id = conversationId,
                MessageId = Guid.NewGuid().ToString("N")
            });
            var result = await _http.Post<AiurValue<Message>>(url, form);
            return result;
        }
    }
}
