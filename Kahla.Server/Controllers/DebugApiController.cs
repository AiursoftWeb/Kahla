using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Kahla.Server.Controllers
{
    public class DebugApiController : ApiController
    {
        public DebugApiController(
            UserManager<KahlaUser> userManager,
            SignInManager<KahlaUser> signInManager,
            KahlaDbContext dbContext,
            PushKahlaMessageService pushService,
            IConfiguration configuration,
            AuthService<KahlaUser> authService,
            ServiceLocation serviceLocation,
            OAuthService oauthService,
            ChannelService channelService,
            StorageService storageService,
            AppsContainer appsContainer,
            UserService userService) : base(
                userManager,
                signInManager,
                dbContext,
                pushService,
                configuration,
                authService,
                serviceLocation,
                oauthService,
                channelService,
                storageService,
                appsContainer,
                userService)
        {
        }
    }
}
