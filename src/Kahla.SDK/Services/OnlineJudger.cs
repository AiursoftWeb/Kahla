using Aiursoft.Scanner.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Kahla.SDK.Services
{
    public class OnlineJudger : ISingletonDependency
    {
        private readonly IMemoryCache _memoryCache;

        public OnlineJudger(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool? IsOnline(string userId, bool allowSeen)
        {
            if (!allowSeen)
            {
                return null;
            }
            var isOnline = false;
            if (_memoryCache.TryGetValue($"last-access-time-{userId}", out DateTime lastAccess))
            {
                isOnline = lastAccess + TimeSpan.FromMinutes(5) > DateTime.UtcNow;
            }
            return isOnline;
        }
    }
}
