using Aiursoft.Pylon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiViewModels
{
    public class IndexViewModel : AiurProtocol
    {
        public string WikiPath { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime UTCTime { get; set; }
    }
}
