﻿using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class MyContactsViewModel : AiurResponse
{
    public List<KahlaUserMappedOthersView> KnownContacts { get; set; } = new();
}

public class MyBlocksViewModel : AiurResponse
{
    public List<KahlaUserMappedOthersView> KnownBlocks { get; set; } = new();
}