using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;

namespace Kahla.SDK.Services
{
    public class ConversationService : IScopedDependency
    {
        private readonly SingletonHTTP _http;
        private readonly KahlaLocation _kahlaLocation;

        public ConversationService(
            SingletonHTTP http,
            KahlaLocation kahlaLocation)
        {
            _http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<AiurCollection<ContactInfo>> AllAsync()
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Conversation", "All", new
            {

            });
            var result = await _http.Get(url);
            var jsonResult = JsonConvert.DeserializeObject<AiurCollection<ContactInfo>>(result);
            if (jsonResult.Code != ErrorType.Success)
            {
                throw new AiurUnexpectedResponse(jsonResult);
            }
            return jsonResult;
        }

        public async Task<AiurCollection<Message>> GetMessagesAsync(int id, int take, string skipFrom)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Conversation", "GetMessage", new
            {
                Id = id,
                Take = take,
                SkipFrom = skipFrom
            });
            var result = await _http.Get(url);
            var jsonResult = JsonConvert.DeserializeObject<AiurCollection<Message>>(result);
            if (jsonResult.Code != ErrorType.Success)
            {
                throw new AiurUnexpectedResponse(jsonResult);
            }
            return jsonResult;
        }

        public async Task<AiurValue<Message>> SendMessageAsync(string content, int conversationId)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Conversation", "SendMessage", new { });
            var form = new AiurUrl(string.Empty, new SendMessageAddressModel
            {
                Content = content,
                Id = conversationId,
                MessageId = Guid.NewGuid().ToString("N")
            });
            var result = await _http.Post(url, form);
            var jResult = JsonConvert.DeserializeObject<AiurValue<Message>>(result);

            if (jResult.Code != ErrorType.Success)
                throw new AiurUnexpectedResponse(jResult);
            return jResult;
        }
    }
}
