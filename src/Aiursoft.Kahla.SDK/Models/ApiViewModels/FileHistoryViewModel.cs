using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class FileHistoryViewModel : AiurCollection<dynamic> // TODO: Use a more specific type than dynamic
    {
        [Obsolete("This method is only for framework", true)]
        public FileHistoryViewModel() { }

        public FileHistoryViewModel(List<dynamic> folders) : base(folders) { }
        public string ShowingDateUtc { get; set; }
        public string SiteName { get; set; }
        public string RootPath { get; set; }
    }
}
