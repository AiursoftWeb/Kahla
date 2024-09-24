using Aiursoft.AiurProtocol;
using Aiursoft.Scanner.Abstractions;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;

namespace Kahla.SDK.Services
{
    public class StorageService : IScopedDependency
    {
        public readonly AiurProtocolClient Http;
        private readonly KahlaLocation _kahlaLocation;

        public StorageService(
            AiurProtocolClient http,
            KahlaLocation kahlaLocation)
        {
            Http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<InitFileAccessViewModel> InitFileAccessAsync(int conversationId, bool canUpload, bool canDownload)
        {
            var url = new AiurApiEndpoint(_kahlaLocation.ToString()!, "Storage", "InitFileAccess", new InitFileAccessAddressModel
            {
                ConversationId = conversationId,
                Upload = canUpload,
                Download = canDownload
            });
            var result = await Http.Get<InitFileAccessViewModel>(url);
            return result;
        }
    }
}
