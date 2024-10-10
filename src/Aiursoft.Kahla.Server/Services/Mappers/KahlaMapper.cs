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
            Name = thread.Name,
            ImagePath = thread.IconFilePath,
            OwnerId = thread.OwnerRelation.UserId,
            AllowDirectJoinWithoutInvitation = thread.AllowDirectJoinWithoutInvitation
        });
    }
    
    public async Task<KahlaThreadMappedJoinedView> MapJoinedThreadAsync(ChatThread thread)
    {
        await Task.CompletedTask; // TODO: In the future, some properties will be calculated here with await.
        return new KahlaThreadMappedJoinedView
        {
            Name = thread.Name,
            ImagePath = thread.IconFilePath,
            OwnerId = thread.OwnerRelation.UserId,
            AllowDirectJoinWithoutInvitation = thread.AllowDirectJoinWithoutInvitation
        };
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

    public async Task<KahlaUserMappedDetailedOthersView> MapDetailedOtherUserView(KahlaUser? user, KahlaUser currentUser)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var commonThreads = await dbContext
            .UserThreadRelations
            .Where(t => t.UserId == currentUser.Id) // My threads
            .Select(t => t.Thread)
            .Where(t => t.Members.Any(u => u.UserId == user.Id)) // Where that user is in
            .ToListAsync();

        var mappedCommonThreads = await commonThreads
            .SelectAsListAsync(MapJoinedThreadAsync);

        return new KahlaUserMappedDetailedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus),
            CommonThreads = mappedCommonThreads
        };
    }
}