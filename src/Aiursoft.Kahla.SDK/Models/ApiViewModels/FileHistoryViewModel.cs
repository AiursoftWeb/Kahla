using Aiursoft.AiurProtocol;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class FileHistoryViewModel : AiurCollection<Aiursoft.Probe.SDK.Models.File>
    {
        [Obsolete("This method is only for framework", true)]
        public FileHistoryViewModel() { }

        public FileHistoryViewModel(List<Aiursoft.Probe.SDK.Models.File> folders) : base(folders) { }
        public string ShowingDateUTC { get; set; }
        public string SiteName { get; set; }
        public string RootPath { get; set; }
    }
}
