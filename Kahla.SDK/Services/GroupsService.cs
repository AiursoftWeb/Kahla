using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;

namespace Kahla.SDK.Services
{
    public class GroupsService : IScopedDependency
    {
        private readonly AiurProtocolClient _http;
        private readonly KahlaLocation _kahlaLocation;

        public GroupsService(
            AiurProtocolClient http,
            KahlaLocation kahlaLocation)
        {
            _http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<AiurValue<int>> JoinGroupAsync(string groupName, string joinPassword)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Groups", "JoinGroup", new { });
            var form = new AiurApiPayload(new
            {
                groupName,
                joinPassword
            });
            var result = await _http.Post<AiurValue<int>>(url, form);
            return result;
        }

        public async Task<AiurResponse> SetGroupMutedAsync(string groupName, bool setMuted)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Groups", "SetGroupMuted", new { });
            var form = new AiurApiPayload(new
            {
                groupName,
                setMuted
            });
            var result = await _http.Post<AiurResponse>(url, form);
            return result;
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