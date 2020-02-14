using Aiursoft.Scanner.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Kahla.SDK.Services
{
    public class LastSaidJudger : ISingletonDependency
    {
        private readonly IMemoryCache _memoryCache;

        public LastSaidJudger(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void MarkSend(string userId, int conversationId)
        {
            _memoryCache.Set($"conversation-{conversationId}-last-said", DateTime.UtcNow);
            _memoryCache.Set($"conversation-{conversationId}-last-said-person", userId);
        }

        public bool ShallBeGroupped(string userId, int conversationId)
        {
            if (_memoryCache.TryGetValue($"conversation-{conversationId}-last-said", out DateTime lastSaid))
            {
                var justSaid = lastSaid + TimeSpan.FromMinutes(2) > DateTime.UtcNow;
                if (justSaid)
                {
                    if (_memoryCache.TryGetValue($"conversation-{conversationId}-last-said-person", out string lastSaidPerson))
                    {
                        if (lastSaidPerson == userId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
