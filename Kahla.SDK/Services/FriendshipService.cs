﻿using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;

namespace Kahla.SDK.Services
{
    public class FriendshipService : IScopedDependency
    {
        private readonly KahlaLocation _kahlaLocation;
        private readonly AiurProtocolClient _http;

        public FriendshipService(
            KahlaLocation kahlaLocation,
            AiurProtocolClient http)
        {
            _kahlaLocation = kahlaLocation;
            _http = http;
        }

        public Task<MineViewModel> MineAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Friendship", "Mine", new { });
            return _http.Get<MineViewModel>(url);
        }

        public async Task<AiurValue<int>> CompleteRequestAsync(int requestId, bool accept)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Friendship", "CompleteRequest", new { });
            var form = new AiurApiPayload(new CompleteRequestAddressModel
            {
                Id = requestId,
                Accept = accept
            });
            var result = await _http.Post<AiurValue<int>>(url, form);
            return result;
        }

        public async Task<AiurCollection<Request>> MyRequestsAsync()
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Friendship", "MyRequests", new { });
            var result = await _http.Get<AiurCollection<Request>>(url);
            return result;
        }
    }
}
