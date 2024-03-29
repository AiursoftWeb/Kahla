﻿using Aiursoft.AiurProtocol;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class VersionViewModel : AiurResponse
    {
        public string LatestVersion { get; set; }
        public string LatestCLIVersion { get; set; }
        public string DownloadAddress { get; set; }
    }
}
