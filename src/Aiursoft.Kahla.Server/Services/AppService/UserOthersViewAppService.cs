using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class UserOthersViewAppService(
    OnlineJudger judger,
    UserOthersViewRepo repo)
{
    public async Task<List<KahlaUserMappedOthersView>> GetMyContactsPagedAsync(string userId, int take)
    {
        var views = await repo.QueryMyContacts(userId, take).ToListAsync();
        views.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return views;
    }
        
    public async Task<List<KahlaUserMappedOthersView>> GetMyBlocksPagedAsync(string userId, int take)
    {
        var views = await repo.QueryMyBlocksPaged(userId, take).ToListAsync();
        views.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return views;
    }

    public async Task<(int count, List<KahlaUserMappedOthersView> users)> SearchUsersPagedAsync(string searchInput, string userId, int take)
    {
        var query = repo.SearchUsers(searchInput, userId);
        var totalCount = await query.CountAsync();
        var users = await query.Take(take).ToListAsync();
        users.ForEach(t => t.Online = judger.IsOnline(t.User.Id, t.User.EnableHideMyOnlineStatus));
        return (totalCount, users);
    }
}