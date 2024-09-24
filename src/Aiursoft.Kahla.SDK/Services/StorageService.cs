using Aiursoft.AiurProtocol;
using Aiursoft.Kahla.SDK.Models.ApiAddressModels;
using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Kahla.SDK.Services
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
