using System.Linq.Expressions;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.Mapped;

namespace Aiursoft.Kahla.Server.Services;

public static class KahlaQueryMapper
{
    public static IQueryable<T> WhereWhen<T>(
        this IQueryable<T> query,
        string? condition,
        Expression<Func<T, bool>> predicate)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return query;
        }
        return query.Where(predicate);
    }
    
    public static IQueryable<KahlaUserMappedOthersView> MapUsersOthersView(this IQueryable<KahlaUser> filteredUsers, string viewingUserId, OnlineJudger onlineJudger)
    {
        return filteredUsers
            .Select(t => new KahlaUserMappedOthersView
            {
                User = t,
                Online = onlineJudger.IsOnline(t.Id, t.EnableHideMyOnlineStatus),
                IsKnownContact = t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = t.BlockedBy.Any(p => p.CreatorId == viewingUserId)
            });
    }
    
    public static IQueryable<KahlaUserMappedInThreadView> MapUsersInThreadView(this IQueryable<KahlaUser> filteredUsers, string viewingUserId, int threadId, OnlineJudger onlineJudger)
    {
        return filteredUsers
            .Select(u => new KahlaUserMappedInThreadView
            {
                User = u,
                Online = onlineJudger.IsOnline(u.Id, u.EnableHideMyOnlineStatus),
                IsKnownContact = u.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = u.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                IsAdmin = u.ThreadsRelations.First(p => p.ThreadId == threadId).UserThreadRole == UserThreadRole.Admin,
                IsOwner = u.ThreadsRelations
                    .Where(p => p.ThreadId == threadId)
                    .Select(p => p.Thread)
                    .First().OwnerRelation!.UserId == viewingUserId,
                JoinTime = u.ThreadsRelations.First(p => p.ThreadId == threadId).JoinTime
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
    
    public static IQueryable<KahlaThreadMappedJoinedView> MapThreadsJoinedView(this IQueryable<ChatThread> filteredThreads, string viewingUserId)
    {
        return filteredThreads
            .Select(t => new KahlaThreadMappedJoinedView
            {
                Id = t.Id,
                Name = t.Name,
                ImagePath = t.IconFilePath,
                OwnerId = t.OwnerRelation!.UserId,
                AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                UnReadAmount = t.Messages.Count(m => m.SendTime > t.Members.SingleOrDefault(u => u.UserId == viewingUserId)!.ReadTimeStamp),
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
                Muted = t.Members.SingleOrDefault(u => u.UserId == viewingUserId)!.Muted,
                TopTenMembers = t.Members
                    .OrderBy(p => p.JoinTime)
                    .Select(p => p.User)
                    .Take(10),
                ImInIt = t.Members.Any(u => u.UserId == viewingUserId),
                ImAdmin = t.Members.SingleOrDefault(u => u.UserId == viewingUserId)!.UserThreadRole == UserThreadRole.Admin,
                ImOwner = t.OwnerRelation.UserId == viewingUserId,
                CreateTime = t.CreateTime
            });
    }
}