using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public class KahlaMapper(
    KahlaDbContext dbContext,
    IMemoryCache memoryCache)
{
    private bool? IsOnline(string userId, bool userEnableHideMyOnlineStatus)
    {
        if (userEnableHideMyOnlineStatus)
        {
            return null;
        }
        var isOnline = false;
        if (memoryCache.TryGetValue($"last-access-time-{userId}", out DateTime lastAccess))
        {
            isOnline = lastAccess + TimeSpan.FromMinutes(5) > DateTime.UtcNow;
        }
        return isOnline;
    }
    
    public Task<KahlaThreadMappedSearchedView> MapSearchedThreadAsync(ChatThread thread)
    {
        return Task.FromResult(new KahlaThreadMappedSearchedView
        {
            Id = thread.Id,
            Name = thread.Name,
            ImagePath = thread.IconFilePath,
            OwnerId = thread.OwnerRelation.UserId,
            AllowDirectJoinWithoutInvitation = thread.AllowDirectJoinWithoutInvitation
        });
    }
    
    public Task<KahlaUserMappedOthersView> MapOtherUserViewAsync(KahlaUser? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        return Task.FromResult(new KahlaUserMappedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus)
        });
    }

    public async Task<KahlaUserMappedDetailedOthersView> MapDetailedOtherUserView(KahlaUser user, KahlaUser currentUser)
    {
        var commonThreads = await dbContext
            .QueryCommonThreads(currentUser.Id, user.Id)
            .ToListAsync();

        if (!dbContext.Entry(user).Collection(t => t.OfKnownContacts).IsLoaded)
        {
            await dbContext.Entry(user)
                .Collection(t => t.OfKnownContacts)
                .LoadAsync();
        }
        if (!dbContext.Entry(user).Collection(t => t.BlockedBy).IsLoaded)
        {
            await dbContext.Entry(user)
                .Collection(t => t.BlockedBy)
                .LoadAsync();
        }

        return new KahlaUserMappedDetailedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus),
            CommonThreads = commonThreads,
            IsKnownContact = user.OfKnownContacts.Any(t => t.CreatorId == currentUser.Id),
            IsBlockedByYou = currentUser.BlockedBy.Any(t => t.CreatorId == user.Id) 
        };
    }
}