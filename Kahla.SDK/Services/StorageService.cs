using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner.Interfaces;
using Aiursoft.XelNaga.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class StorageService : IScopedDependency
    {
        public readonly SingletonHTTP Http;
        private readonly KahlaLocation _kahlaLocation;

        public StorageService(
            SingletonHTTP http,
            KahlaLocation kahlaLocation)
        {
            Http = http;
            _kahlaLocation = kahlaLocation;
        }

        public async Task<InitFileAccessViewModel> InitFileAccessAsync(int conversationId, bool canUpload, bool canDownload)
        {
            var url = new AiurUrl(_kahlaLocation.ToString(), "Storage", "InitFileAccess", new InitFileAccessAddressModel
            {
                ConversationId = conversationId,
                Upload = canUpload,
                Download = canDownload
            });
            var result = await Http.Get(url);
            var JResult = JsonConvert.DeserializeObject<InitFileAccessViewModel>(result);

            if (JResult.Code != ErrorType.Success)
                throw new AiurUnexpectedResponse(JResult);
            return JResult;
        }
    }
}
