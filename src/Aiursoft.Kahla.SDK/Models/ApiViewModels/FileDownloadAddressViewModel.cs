﻿using Aiursoft.AiurProtocol;
using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class FileDownloadAddressViewModel : AiurResponse
    {
        public string FileName { get; set; }
        public string DownloadPath { get; set; }
    }
}
