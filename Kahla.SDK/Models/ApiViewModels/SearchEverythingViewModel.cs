﻿using Aiursoft.AiurProtocol;

namespace Kahla.SDK.Models.ApiViewModels
{
    public class SearchEverythingViewModel : AiurResponse
    {
        public int UsersCount { get; set; }
        public int GroupsCount { get; set; }
        public List<KahlaUser> Users { get; set; }
        public List<SearchedGroup> Groups { get; set; }
    }
}
