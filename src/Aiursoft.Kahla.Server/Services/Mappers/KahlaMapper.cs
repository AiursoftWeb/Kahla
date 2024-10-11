using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public class KahlaMapper(
    ILogger<KahlaMapper> logger,
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
    
    /// <summary>
    /// Map a KahlaUser to KahlaUserMappedOthersView.
    ///
    /// Before calling this method, it is suggested to load the OfKnownContacts and BlockedBy collections.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public async Task<KahlaUserMappedOthersView> MapOtherUserViewAsync(KahlaUser user, string currentUserId)
    {
        if (!dbContext.Entry(user).Collection(t => t.OfKnownContacts).IsLoaded)
        {
            logger.LogWarning("OfKnownContacts is not loaded for user: {UserId}. Loading...", user.Id);
            await dbContext.Entry(user)
                .Collection(t => t.OfKnownContacts)
                .LoadAsync();
        }
        if (!dbContext.Entry(user).Collection(t => t.BlockedBy).IsLoaded)
        {
            logger.LogWarning("BlockedBy is not loaded for user: {UserId}. Loading...", user.Id);
            await dbContext.Entry(user)
                .Collection(t => t.BlockedBy)
                .LoadAsync();
        }
        
        return new KahlaUserMappedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus),
            IsKnownContact = user.OfKnownContacts.Any(t => t.CreatorId == currentUserId),
            IsBlockedByYou = user.BlockedBy.Any(t => t.CreatorId == currentUserId) 
        };
    }

    public async Task<KahlaUserMappedDetailedOthersView> MapDetailedOtherUserView(KahlaUser user, string currentUserId)
    {
        var commonThreads = await dbContext
            .QueryCommonThreads(currentUserId, user.Id)
            .ToListAsync();
        
        if (!dbContext.Entry(user).Collection(t => t.OfKnownContacts).IsLoaded)
        {
            logger.LogWarning("OfKnownContacts is not loaded for user: {UserId}. Loading...", user.Id);
            await dbContext.Entry(user)
                .Collection(t => t.OfKnownContacts)
                .LoadAsync();
        }
        if (!dbContext.Entry(user).Collection(t => t.BlockedBy).IsLoaded)
        {
            logger.LogWarning("BlockedBy is not loaded for user: {UserId}. Loading...", user.Id);
            await dbContext.Entry(user)
                .Collection(t => t.BlockedBy)
                .LoadAsync();
        }
        return new KahlaUserMappedDetailedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus),
            CommonThreads = commonThreads,
            IsKnownContact = user.OfKnownContacts.Any(t => t.CreatorId == currentUserId),
            IsBlockedByYou = user.BlockedBy.Any(t => t.CreatorId == currentUserId) 
        };
    }
}