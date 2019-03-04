using Aiursoft.Pylon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiViewModels
{
    public class FileDownloadAddressViewModel : AiurProtocol
    {
        public string FileName { get; set; }
        public string DownloadPath { get; set; }
    }
}
