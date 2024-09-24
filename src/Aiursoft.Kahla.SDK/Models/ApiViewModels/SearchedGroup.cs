namespace Aiursoft.Kahla.SDK.Models.ApiViewModels
{
    public class SearchedGroup
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

        [Obsolete(error: true, message: "This method is only for doc generator.")]
        public SearchedGroup()
        {

        }

        public SearchedGroup(GroupConversation conversation)
        {
            ImagePath = conversation.GroupImagePath;
            Name = conversation.GroupName;
            HasPassword = !string.IsNullOrEmpty(conversation.JoinPassword);
            OwnerId = conversation.OwnerId;
            Id = conversation.Id;
            ConversationCreateTime = conversation.ConversationCreateTime;
        }

        public string ImagePath { get; set; }
        public string Name { get; set; }
        public bool HasPassword { get; set; }
        public string OwnerId { get; set; }
        public int Id { get; set; }
        public DateTime ConversationCreateTime { get; set; }
    }
}
