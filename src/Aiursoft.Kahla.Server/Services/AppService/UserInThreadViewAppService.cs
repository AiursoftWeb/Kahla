using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class UserInThreadViewAppService(UserInThreadViewRepo repo)
{
    public async Task<(int count, List<KahlaUserMappedInThreadView> members)> QueryMembersInThreadAsync(int threadId, string viewingUserId, int skip, int take)
    {
        var query = repo.QueryMembersInThread(threadId, viewingUserId);
        var count = await query.CountAsync();
        var members = await query.Skip(skip).Take(take).ToListAsync();
        return (count, members);
    }
}