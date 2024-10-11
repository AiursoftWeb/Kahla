using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.AppService;

public class UserDetailedViewAppService(
    ThreadJoinedViewAppService threadService,
    UserOthersViewRepo userOthersViewRepo)
{
    public async Task<KahlaUserMappedDetailedView?> GetUserDetailedViewAsync(string targetUser, string currentUser, int takeThreads)
    {
        var commonThreads = await threadService.QueryCommonThreadsAsync(
            viewingUserId: currentUser,
            targetUserId: targetUser,
            take: takeThreads);

        var user = await userOthersViewRepo.QueryUserById(
                targetUserId: targetUser,
                viewingUserId: currentUser)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return null;
        }

        return new KahlaUserMappedDetailedView
        {
            SearchedUser = user,
            CommonThreads = commonThreads.threads,
        };
    }
}
