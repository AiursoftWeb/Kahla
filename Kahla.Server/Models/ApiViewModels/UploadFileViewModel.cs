using Aiursoft.Pylon.Models;

namespace Kahla.Server.Models.ApiViewModels
{
    public class UploadFileViewModel : AiurProtocol
    {
        public string SavedFileName { get; set; }
        public int FileKey { get; set; }
        public long FileSize { get; set; }
    }

    public class UploadImageViewModel : AiurProtocol
    {
        public int FileKey { get; set; }
        public string DownloadPath { get; set; }
    }
}
