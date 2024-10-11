using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class ThreadJoinedViewRepo(KahlaDbContext dbContext)
{
    private IQueryable<KahlaThreadMappedJoinedView> MapThreadsJoinedView(IQueryable<ChatThread> filteredThreads, string currentUserId)
    {
        return filteredThreads
            .Select(t => new KahlaThreadMappedJoinedView
            {
                Id = t.Id,
                Name = t.Name,
                ImagePath = t.IconFilePath,
                OwnerId = t.OwnerRelation.UserId,
                AllowDirectJoinWithoutInvitation = t.AllowDirectJoinWithoutInvitation,
                UnReadAmount = t.Messages.Count(m => m.SendTime > t.Members.SingleOrDefault(u => u.UserId == currentUserId)!.ReadTimeStamp),
                LatestMessage = t.Messages
                    .OrderByDescending(p => p.SendTime)
                    .FirstOrDefault(),
                LatestMessageSender = t.Messages.Any() ? t.Messages
                    .OrderByDescending(p => p.SendTime)
                    .Select(m => m.Sender)
                    .FirstOrDefault() : null,
                Muted = t.Members.SingleOrDefault(u => u.UserId == currentUserId)!.Muted,
                TopTenMembers = t.Members
                    .OrderBy(p => p.JoinTime)
                    .Select(p => p.User)
                    .Take(10),
                ImInIt = t.Members.Any(u => u.UserId == currentUserId)
            });
    }

    public IQueryable<KahlaThreadMappedJoinedView> QueryCommonThreads(string viewingUserId, string targetUserId)
    {
        var query = dbContext.ChatThreads
            .AsNoTracking()
            .Where(t => t.Members.Any(p => p.UserId == viewingUserId))
            .Where(t => t.Members.Any(p => p.UserId == targetUserId));
        return MapThreadsJoinedView(query, viewingUserId);
    }
}