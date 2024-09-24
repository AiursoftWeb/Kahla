using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class FileHistoryViewModel : AiurCollection<dynamic> // TODO: Use a more specific type than dynamic
    {
        public FileHistoryViewModel(List<dynamic> folders) : base(folders) { }
    }
}
