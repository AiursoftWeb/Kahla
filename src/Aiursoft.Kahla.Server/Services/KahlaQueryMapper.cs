using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.Server.Services;

public static class KahlaQueryMapper
{
    public static IQueryable<KahlaUserMappedOthersView> MapUsersOthersView(this IQueryable<KahlaUser> filteredUsers, string viewingUserId)
    {
        return filteredUsers
            .Select(t => new KahlaUserMappedOthersView
            {
                User = t,
                Online = null, // This needed to be calculated in real-time.
                IsKnownContact = t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = t.BlockedBy.Any(p => p.CreatorId == viewingUserId)
            });
    }
    
    public static IQueryable<KahlaThreadMappedOthersView> MapThreadsOthersView(this IQueryable<ChatThread> filteredThreads, string userId)
    {
        return filteredThreads
            .Select(t => new KahlaThreadMappedOthersView
            {
                Id = t.Id,
                Name = t.Name,
                ImagePath = t.IconFilePath,
                OwnerId = t.OwnerRelation!.UserId,
                AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                CreateTime = t.CreateTime,
                ImInIt = t.Members.Any(u => u.UserId == userId)
            });
    }
    
    public static IQueryable<KahlaThreadMappedJoinedView> MapThreadsJoinedView(this IQueryable<ChatThread> filteredThreads, string currentUserId)
    {
        return filteredThreads
            .Select(t => new KahlaThreadMappedJoinedView
            {
                Id = t.Id,
                Name = t.Name,
                ImagePath = t.IconFilePath,
                OwnerId = t.OwnerRelation!.UserId,
                AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                UnReadAmount = t.Messages.Count(m => m.SendTime > t.Members.SingleOrDefault(u => u.UserId == currentUserId)!.ReadTimeStamp),
                LatestMessage = t.Messages
                    .OrderByDescending(p => p.SendTime)
                    .FirstOrDefault(),
                LastMessageTime = t.Messages.Any() ? t.Messages
                    .OrderByDescending(p => p.SendTime)
                    .Select(m => m.SendTime)
                    .FirstOrDefault() : t.CreateTime,
                LatestMessageSender = t.Messages.Any() ? t.Messages
                    .OrderByDescending(p => p.SendTime)
                    .Select(m => m.Sender)
                    .FirstOrDefault() : null,
                Muted = t.Members.SingleOrDefault(u => u.UserId == currentUserId)!.Muted,
                TopTenMembers = t.Members
                    .OrderBy(p => p.JoinTime)
                    .Select(p => p.User)
                    .Take(10),
                ImInIt = t.Members.Any(u => u.UserId == currentUserId),
                ImAdmin = t.Members.SingleOrDefault(u => u.UserId == currentUserId)!.UserThreadRole == UserThreadRole.Admin,
                ImOwner = t.OwnerRelation.UserId == currentUserId,
                CreateTime = t.CreateTime
            });
    }
}