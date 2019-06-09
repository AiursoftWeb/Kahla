using System;
using System.Collections.Generic;
using System.Linq;

namespace Kahla.Server.Models.ApiViewModels
{
    public class SearchedGroup
    {
        public static List<SearchedGroup> Map(List<GroupConversation> conversations, string userId)
        {
            var list = new List<SearchedGroup>();
            foreach (var conversation in conversations)
            {
                list.Add(new SearchedGroup(conversation, userId));
            }
            return list;
        }

        private SearchedGroup(GroupConversation conversation, string currentUserId)
        {
            ImageKey = conversation.GroupImageKey;
            Name = conversation.GroupName;
            HasPassword = !string.IsNullOrEmpty(conversation.JoinPassword);
            OwnerId = conversation.OwnerId;
            Id = conversation.Id;
            HasTimer = conversation.MaxLiveSeconds < int.MaxValue;
            ConversationCreateTime = conversation.ConversationCreateTime;
        }
        public int ImageKey { get; set; }
        public string Name { get; set; }
        public bool HasPassword { get; set; }
        public string OwnerId { get; set; }
        public int Id { get; set; }
        public bool HasTimer { get; set; }
        public DateTime ConversationCreateTime { get; set; }
    }
}
