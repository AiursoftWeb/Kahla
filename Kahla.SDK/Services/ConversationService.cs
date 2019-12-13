using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

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
                throw new AiurUnexceptedResponse(jsonResult);
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
                throw new AiurUnexceptedResponse(jsonResult);
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
                MessageId = Guid.NewGuid().ToString("N"),
                RecordTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.7)
            });
            var result = await _http.Post(url, form);
            var JResult = JsonConvert.DeserializeObject<AiurValue<Message>>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexceptedResponse(JResult);
            return JResult;
        }
    }
}
