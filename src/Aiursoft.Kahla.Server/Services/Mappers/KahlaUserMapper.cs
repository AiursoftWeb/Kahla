using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public class KahlaUserMapper(IMemoryCache memoryCache)
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

    public KahlaUserMappedOthersViewWithCommonThreads MapDetailedView(KahlaUser? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return new KahlaUserMappedOthersViewWithCommonThreads
        {
            User = user,
            Online = IsOnline(user.Id, userEnableHideMyOnlineStatus: user.EnableHideMyOnlineStatus)
        };
    }
}