using Aiursoft.Pylon.Models;
using System;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class IndexViewModel : AiurProtocol
    {
        public string WikiPath { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime UTCTime { get; set; }
    }
}
