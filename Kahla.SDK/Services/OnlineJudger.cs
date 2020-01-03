using Aiursoft.XelNaga.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Kahla.SDK.Services
{
    public class OnlineJudger : ISingletonDependency
    {
        private readonly IMemoryCache _memoryCache;

        public OnlineJudger(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool IsOnline(string userId)
        {
            var isOnline = false;
            if (_memoryCache.TryGetValue(userId, out DateTime lastAccess))
            {
                isOnline = lastAccess + TimeSpan.FromMinutes(5) > DateTime.UtcNow;
            }
            return isOnline;
        }
    }
}
