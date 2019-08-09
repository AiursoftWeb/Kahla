using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class UploadFileViewModel : AiurProtocol
    {
        public string FilePath { get; set; }
        public long FileSize { get; set; }
    }

    public class UploadImageViewModel : AiurProtocol
    {
        public string FilePath { get; set; }
    }
}
