using Aiursoft.Pylon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Pylon;

namespace Kahla.Server.Models.ApiViewModels
{
    public class VersionViewModel : AiurProtocol
    {
        public string LatestVersion { get; set; }
        public string OldestSupportedVersion { get; set; }
        public string DownloadAddress { get; set; }
    }
}
