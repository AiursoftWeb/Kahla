using Aiursoft.Canon;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Data;

public class DevicesCache(
    ILogger<DevicesCache> logger,
    KahlaRelationalDbContext context,
    CacheService cache)
{
    public async Task<List<Device>> GetValidDevicesWithCache(string userId)
    {
        return await cache.RunWithCache($"user-with-ids-devices-{userId}", async () =>
        {
            logger.LogTrace("Devices cache missed! Loading devices for user: {UserId}", userId);
            return await context
                .Devices
                .AsNoTracking()
                .Where(t => t.OwnerId == userId)
                .ToListAsync();
        }, cacheCondition: r => r.Count != 0, cachedMinutes: _ => TimeSpan.FromMinutes(60));
    }
    
    public void ClearCacheForUser(string userId)
    {
        logger.LogInformation("Clearing cached devices for user: {UserId}...", userId);
        cache.Clear($"user-with-ids-devices-{userId}");
    }
}