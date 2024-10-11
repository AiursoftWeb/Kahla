using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Services;

public class OnlineJudger(
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
}