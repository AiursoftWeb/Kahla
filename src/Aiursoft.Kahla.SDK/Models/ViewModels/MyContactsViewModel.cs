﻿using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MyContactsViewModel : AiurResponse
{
    public List<KahlaUserMappedOthersView> Users { get; set; } = new();
}