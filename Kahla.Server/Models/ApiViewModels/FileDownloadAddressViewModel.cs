using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class FileDownloadAddressViewModel : AiurProtocol
    {
        public string FileName { get; set; }
        public string DownloadPath { get; set; }
    }
}
