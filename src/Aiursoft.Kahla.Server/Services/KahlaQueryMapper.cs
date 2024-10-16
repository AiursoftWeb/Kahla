using System.Linq.Expressions;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;

namespace Aiursoft.Kahla.Server.Services;

public static class KahlaQueryMapper
{
    public static IQueryable<KahlaUserMappedOthersView> MapUsersOthersView(this IQueryable<KahlaUser> filteredUsers, string viewingUserId, OnlineJudger onlineJudger)
    {
        return filteredUsers
            .Select(t => new KahlaUserMappedOthersView
            {
                User = t,
                IsKnownContact = t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = t.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                Online = onlineJudger.IsOnline(t.Id, t.EnableHideMyOnlineStatus) // Client side evaluate.
            });
    }
    
    public static IQueryable<KahlaUserMappedInThreadView> MapUsersInThreadView(this IQueryable<UserThreadRelation> filteredRelations, string viewingUserId, OnlineJudger onlineJudger)
    {
        return filteredRelations
            .Select(u => new KahlaUserMappedInThreadView
            {
                User = u.User,
                IsKnownContact = u.User.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = u.User.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                IsAdmin = u.UserThreadRole == UserThreadRole.Admin,
                IsOwner = u.Thread.OwnerRelation!.UserId == viewingUserId,
                JoinTime = u.JoinTime,
                Online = onlineJudger.IsOnline(u.UserId, u.User.EnableHideMyOnlineStatus) // Client side evaluate.
            });
    }
    
    public static IQueryable<KahlaThreadMappedOthersView> MapThreadsOthersView(this IQueryable<ChatThread> filteredThreads, string viewingUserId)
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
                ImInIt = t.Members.Any(u => u.UserId == viewingUserId)
            });
    }
    
    public static IQueryable<KahlaThreadMappedJoinedView> MapThreadsJoinedView(
        this IQueryable<ChatThread> filteredThreads, 
        string viewingUserId, 
        OnlineJudger onlineJudger,
        QuickMessageAccess quickMessageAccess)
    {
        return filteredThreads
            .Select(t => new KahlaThreadMappedJoinedView
            {
                Id = t.Id,
                Name = t.Name,
                ImagePath = t.IconFilePath,
                OwnerId = t.OwnerRelation!.UserId,
                AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                Muted = t.Members.SingleOrDefault(u => u.UserId == viewingUserId)!.Muted,
                TopTenMembers = t.Members
                    .Select(u => new KahlaUserMappedInThreadView
                    {
                        User = u.User,
                        IsKnownContact = u.User.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                        IsBlockedByYou = u.User.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                        IsAdmin = u.UserThreadRole == UserThreadRole.Admin,
                        IsOwner = t.OwnerRelationId == u.Id,
                        JoinTime = u.JoinTime,
                        Online = onlineJudger.IsOnline(u.UserId, u.User.EnableHideMyOnlineStatus) // Client side evaluate.
                    })
                    .OrderBy(u => u.JoinTime)
                    .Take(10),
                ImInIt = t.Members.Any(u => u.UserId == viewingUserId),
                ImAdmin = t.Members.SingleOrDefault(u => u.UserId == viewingUserId)!.UserThreadRole ==
                          UserThreadRole.Admin,
                ImOwner = t.OwnerRelation.UserId == viewingUserId,
                CreateTime = t.CreateTime,
                AllowMemberSoftInvitation = t.AllowMemberSoftInvitation,
                AllowMembersSendMessages = t.AllowMembersSendMessages,
                AllowMembersEnlistAllMembers = t.AllowMembersEnlistAllMembers,
                AllowSearchByName = t.AllowSearchByName,
                LastMessageTime = t.LastMessageTime,
                MessageContext = quickMessageAccess.GetMessageContext(t.Id, viewingUserId) // Client side evaluate.
            });
    }
}
