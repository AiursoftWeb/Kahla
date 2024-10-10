using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public class KahlaUserMapper(
    KahlaThreadMapper kahlaThreadMapper,
    KahlaDbContext dbContext,
    IMemoryCache memoryCache)
{
    public bool? IsOnline(string userId, bool userEnableHideMyOnlineStatus)
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
    
    public KahlaUserMappedOthersView MapOthersView(KahlaUser? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        return new KahlaUserMappedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus)
        };
    }

    public async Task<KahlaUserMappedDetailedOthersView> MapDetailedOthersView(KahlaUser? user, KahlaUser currentUser)
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
            .SelectAsListAsync(kahlaThreadMapper.MapJoinedThread);

        return new KahlaUserMappedDetailedOthersView
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus),
            CommonThreads = mappedCommonThreads
        };
    }
}

public static class EnumerableExtensions
{
    public static async Task<List<T2>> SelectAsListAsync<T1, T2>(this IEnumerable<T1> source, Func<T1, Task<T2>> selector)
    {
        var result = new List<T2>();
        foreach (var item in source)
        {
            result.Add(await selector(item));
        }
        return result;
    }
}