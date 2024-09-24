using Aiursoft.Kahla.SDK.Models.Conversations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiViewModels
{
    public class SearchedGroup(GroupConversation conversation)
    {
        public static List<SearchedGroup> Map(List<GroupConversation> conversations)
        {
            var list = new List<SearchedGroup>();
            foreach (var conversation in conversations)
            {
                list.Add(new SearchedGroup(conversation));
            }
            return list;
        }

        public string ImagePath { get; set; } = conversation.GroupImagePath;
        public string Name { get; set; } = conversation.GroupName;
        public bool HasPassword { get; set; } = !string.IsNullOrEmpty(conversation.JoinPassword);
        public string OwnerId { get; set; } = conversation.OwnerId;
        public int Id { get; set; } = conversation.Id;
        public DateTime ConversationCreateTime { get; set; } = conversation.ConversationCreateTime;
    }
}
