using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public static class KahlaQueryMappers
{
    public static IQueryable<KahlaUserMappedOthersView> MapUsersOthersView(this IQueryable<KahlaUser> filteredUsers,
        string viewingUserId, OnlineDetector onlineDetector)
    {
        return filteredUsers
            .Select(t => new KahlaUserMappedOthersView
            {
                User = new KahlaUserMappedPublicView
                {
                    Id = t.Id,
                    NickName = t.NickName,
                    Bio = t.Bio,
                    IconFilePath = t.IconFilePath,
                    AccountCreateTime = t.AccountCreateTime,
                    EmailConfirmed = t.EmailConfirmed,
                    Email = t.Email
                },
                IsKnownContact = t.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = t.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                Online = onlineDetector.IsOnline(t.Id, t.EnableHideMyOnlineStatus) // Client side evaluate.
            });
    }

    public static IQueryable<KahlaUserMappedInThreadView> MapUsersInThreadView(
        this IQueryable<UserThreadRelation> filteredRelations, string viewingUserId, OnlineDetector onlineDetector)
    {
        return filteredRelations
            .Select(ur => new KahlaUserMappedInThreadView
            {
                User = new KahlaUserMappedPublicView
                {
                    Id = ur.UserId,
                    NickName = ur.User.NickName,
                    Bio = ur.User.Bio,
                    IconFilePath = ur.User.IconFilePath,
                    AccountCreateTime = ur.User.AccountCreateTime,
                    EmailConfirmed = ur.User.EmailConfirmed,
                    Email = ur.User.Email
                },
                IsKnownContact = ur.User.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                IsBlockedByYou = ur.User.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                IsAdmin = ur.UserThreadRole == UserThreadRole.Admin,
                IsOwner = ur.Thread.OwnerRelationId == ur.Id,
                JoinTime = ur.JoinTime,
                Online = onlineDetector.IsOnline(ur.UserId, ur.User.EnableHideMyOnlineStatus) // Client side evaluate.
            });
    }

    public static IQueryable<KahlaThreadMappedOthersView> MapThreadsOthersView(
        this IQueryable<ChatThread> filteredThreads, string viewingUserId)
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
        OnlineDetector onlineDetector,
        QuickMessageAccess quickMessageAccess,
        ArrayDbContext arrayDb)
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
                        User = new KahlaUserMappedPublicView
                        {
                            Id = u.UserId,
                            NickName = u.User.NickName,
                            Bio = u.User.Bio,
                            IconFilePath = u.User.IconFilePath,
                            AccountCreateTime = u.User.AccountCreateTime,
                            EmailConfirmed = u.User.EmailConfirmed,
                            Email = u.User.Email
                        },
                        IsKnownContact = u.User.OfKnownContacts.Any(p => p.CreatorId == viewingUserId),
                        IsBlockedByYou = u.User.BlockedBy.Any(p => p.CreatorId == viewingUserId),
                        IsAdmin = u.UserThreadRole == UserThreadRole.Admin,
                        IsOwner = t.OwnerRelationId == u.Id,
                        JoinTime = u.JoinTime,
                        Online = onlineDetector.IsOnline(u.UserId,
                            u.User.EnableHideMyOnlineStatus) // Client side evaluate.
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
                TotalMessages = (uint)arrayDb.GetTotalMessagesCount(t.Id),
                MessageContext = quickMessageAccess.GetMessageContext(t.Id, viewingUserId) // Client side evaluate.
            });
    }

    public static IQueryable<DeviceMappedOwnerView> MapDevicesOwnedView(this IQueryable<Device> filteredDevices)
    {
        return filteredDevices
            .Select(t => new DeviceMappedOwnerView
            {
                Id = t.Id,
                Name = t.Name,
                AddTime = t.AddTime,
                IpAddress = t.IpAddress
            });
    }
}