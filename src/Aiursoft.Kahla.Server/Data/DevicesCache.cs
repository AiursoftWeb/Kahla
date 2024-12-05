using Aiursoft.Canon;
using Aiursoft.Kahla.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Data;

public class DevicesCache(
    KahlaRelationalDbContext context,
    CacheService cache)
{
    public async Task<List<Device>> GetValidDevicesWithCache(string userId)
    {
        return await cache.RunWithCache($"user-with-ids-devices-{userId}", async () =>
        {
            return await context
                .Devices
                .AsNoTracking()
                .Where(t => t.OwnerId == userId)
                .ToListAsync();
        }, cacheCondition: r => r.Count != 0, cachedMinutes: _ => TimeSpan.FromMinutes(60));
    }
    
    public void ClearCacheForUser(string userId)
    {
        cache.Clear($"user-with-ids-devices-{userId}");
    }
}