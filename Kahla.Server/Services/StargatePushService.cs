using Aiursoft.Archon.SDK.Services;
using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner.Interfaces;
using Aiursoft.Stargate.SDK.Models.ChannelViewModels;
using Aiursoft.Stargate.SDK.Services.ToStargateServer;
using Kahla.Server.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class StargatePushService : IScopedDependency
    {
        private readonly ChannelService _channelService;
        private readonly AppsContainer _appsContainer;
        private readonly PushMessageService _pushMessageService;
        private readonly KahlaDbContext _kahlaDbContext;

        public StargatePushService(
            ChannelService channelService,
            AppsContainer appsContainer,
            PushMessageService pushMessageService,
            KahlaDbContext kahlaDbContext)
        {
            _channelService = channelService;
            _appsContainer = appsContainer;
            _pushMessageService = pushMessageService;
            _kahlaDbContext = kahlaDbContext;
        }
        public async Task<CreateChannelViewModel> ReCreateStargateChannel(string userId)
        {
            var token = await _appsContainer.AccessToken();
            var channel = await _channelService.CreateChannelAsync(token, $"Kahla User Channel for Id: {userId}");
            return channel;
        }

        public async Task<AiurProtocol> PushMessageAsync(string accessToken, int channelId, object eventObject)
        {
            try
            {
                return await _pushMessageService.PushMessageAsync(accessToken, channelId, eventObject);
            }
            catch (AiurUnexpectedResponse e) when (e.Code == ErrorType.NotFound)
            {
                var referenced = _kahlaDbContext.Users.Where(t => t.CurrentChannel == channelId);
                foreach (var user in referenced)
                {
                    user.CurrentChannel = 0;
                    _kahlaDbContext.Users.Update(user);
                }
                await _kahlaDbContext.SaveChangesAsync();
                throw;
            }
        }
    }
}
