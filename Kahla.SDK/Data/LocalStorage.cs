using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kahla.SDK.Data
{
    public class LocalStorage : ISingletonDependency
    {
        private readonly ConversationService _conversationService;

        public List<ContactInfo> Contacts { get; set; }

        public LocalStorage(ConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task Init()
        {
            if (Contacts == null)
            {
                var allResponse = await _conversationService.AllAsync();
                Contacts = allResponse.Items;
            }
        }
    }
}
