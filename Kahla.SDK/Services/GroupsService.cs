using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;

namespace Kahla.SDK.Services
{
    public class GroupsService : IScopedDependency
    {
        private readonly SingletonHTTP _http;
        private readonly KahlaLocation _kahlaLocation;

        public GroupsService(
            SingletonHTTP http,
            KahlaLocation kahlaLocation)
        {
            _http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<AiurValue<int>> JoinGroupAsync(string groupName, string joinPassword)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Groups", "JoinGroup", new { });
            var form = new AiurUrl(string.Empty, new
            {
                groupName,
                joinPassword
            });
            var result = await _http.Post(url, form);
            var jResult = JsonConvert.DeserializeObject<AiurValue<int>>(result);

            if (jResult.Code != ErrorType.Success)
                throw new AiurUnexpectedResponse(jResult);
            return jResult;
        }

        public async Task<AiurProtocol> SetGroupMutedAsync(string groupName, bool setMuted)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Groups", "SetGroupMuted", new { });
            var form = new AiurUrl(string.Empty, new
            {
                groupName,
                setMuted
            });
            var result = await _http.Post(url, form);
            var jResult = JsonConvert.DeserializeObject<AiurValue<AiurProtocol>>(result);

            if (jResult.Code != ErrorType.Success && jResult.Code != ErrorType.HasSuccessAlready)
                throw new AiurUnexpectedResponse(jResult);
            return jResult;
        }

        public async Task<AiurValue<SearchedGroup>> GroupSummaryAsync(int groupId)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Groups", "GroupSummary", new
            {
                id = groupId
            });
            var result = await _http.Get(url);
            var jResult = JsonConvert.DeserializeObject<AiurValue<SearchedGroup>>(result);
            if (jResult.Code != ErrorType.Success)
                throw new AiurUnexpectedResponse(jResult);
            return jResult;
        }
    }
}