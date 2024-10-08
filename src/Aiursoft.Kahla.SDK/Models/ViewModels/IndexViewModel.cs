﻿using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels
{
    public class IndexViewModel : AiurResponse
    {
        public required string VapidPublicKey { get; set; }
        public required string ServerName { get; set; }
        public bool AutoAcceptRequests { get; set; }
    }
}
