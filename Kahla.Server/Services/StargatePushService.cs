using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner.Abstract;
using Aiursoft.Stargate.SDK.Models.ChannelViewModels;
using Aiursoft.Stargate.SDK.Services.ToStargateServer;
using Kahla.Server.Data;
using System.Linq;
using System.Threading.Tasks;
using Aiursoft.Directory.SDK.Services;

namespace Kahla.Server.Services
{
    public class StargatePushService : IScopedDependency
    {
        private readonly ChannelService _channelService;
        private readonly DirectoryAppTokenService _appsContainer;
        private readonly PushMessageService _pushMessageService;
        private readonly KahlaDbContext _kahlaDbContext;

        public StargatePushService(
            ChannelService channelService,
            DirectoryAppTokenService appsContainer,
            PushMessageService pushMessageService,
            KahlaDbContext kahlaDbContext)
        {
            _channelService = channelService;
            _appsContainer = appsContainer;
            _pushMessageService = pushMessageService;
            _kahlaDbContext = kahlaDbContext;
        }

        public async Task<CreateChannelViewModel> ReCreateStargateChannel()
        {
            var token = await _appsContainer.GetAccessTokenAsync();
            var channel = await _channelService.CreateChannelAsync(token, $"Kahla User Channel");
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
                    user.CurrentChannel = -1;
                    _kahlaDbContext.Users.Update(user);
                }
                await _kahlaDbContext.SaveChangesAsync();
                throw;
            }
        }
    }
}
