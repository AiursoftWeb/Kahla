using Aiursoft.AiurProtocol;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class InitFileAccessViewModel : AiurValue<string>
    {
        public InitFileAccessViewModel(string input) : base(input) { }

        public string UploadAddress { get; set; }
    }
}
