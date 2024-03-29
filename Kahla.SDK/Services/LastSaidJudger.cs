﻿using Aiursoft.Scanner.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Kahla.SDK.Services
{
    public class LastSaidJudger : ISingletonDependency
    {
        private readonly IMemoryCache _memoryCache;

        public LastSaidJudger(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void MarkSend(string userId, int conversationId, Guid messageId)
        {
            _memoryCache.Set($"conversation-{conversationId}-last-said", DateTime.UtcNow);
            _memoryCache.Set($"conversation-{conversationId}-last-said-person", userId);
            _memoryCache.Set($"conversation-{conversationId}-last-said-message", messageId.ToString());
        }

        public string LastMessageId(int conversationId)
        {
            if (_memoryCache.TryGetValue($"conversation-{conversationId}-last-said-message", out string lastMessageId))
            {
                return lastMessageId;
            }
            throw new InvalidOperationException("Last message is not in memory.");
        }

        public bool ShallBeGroupped(string userId, int conversationId)
        {
            if (_memoryCache.TryGetValue($"conversation-{conversationId}-last-said", out DateTime lastSaid))
            {
                var justSaid = lastSaid + TimeSpan.FromHours(1) > DateTime.UtcNow;
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
