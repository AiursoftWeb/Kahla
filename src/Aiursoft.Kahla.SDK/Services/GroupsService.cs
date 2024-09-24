using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Services;
using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Kahla.SDK.Services
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

        public Task<AiurValue<SearchedGroup>> GroupSummaryAsync(int groupId)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Groups", "GroupSummary", new
            {
                id = groupId
            });
            return _http.Get<AiurValue<SearchedGroup>>(url);
        }
    }
}