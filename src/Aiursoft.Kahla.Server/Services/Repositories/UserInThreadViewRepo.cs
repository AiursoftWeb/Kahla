using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class UserInThreadViewRepo(KahlaRelationalDbContext relationalDbContext, OnlineDetector onlineDetector)
{
    public IOrderedQueryable<KahlaUserMappedInThreadView> QueryMembersInThread(
        int threadId, string? searchInput, string? excluding, string viewingUserId)
    {
        return relationalDbContext.UserThreadRelations
            .AsNoTracking()
            .Where(t => t.ThreadId == threadId)
            .WhereWhen(excluding, t => !t.User.NickName.Contains(excluding!))
            .WhereWhen(searchInput, t =>
                t.User.NickName.Contains(searchInput!) ||
                t.User.Id.ToString() == searchInput ||
                t.User.Email.Contains(searchInput!))
            .MapUsersInThreadView(viewingUserId, onlineDetector)
            .OrderBy(t => t.JoinTime);
    }
}