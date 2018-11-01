using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.ForApps.AddressModels;
using Aiursoft.Pylon.Models.Stargate.ListenAddressModels;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToAPIServer;
using Aiursoft.Pylon.Services.ToOSSServer;
using Aiursoft.Pylon.Services.ToStargateServer;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    public static class KahlaExtends
    {
        public static JsonResult AiurJson(this Controller controller ,object obj)
        {
            return controller.Json(obj, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
        }
    }

    [APIExpHandler]
    [APIModelStateChecker]
    public class ApiController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly SignInManager<KahlaUser> _signInManager;
        private readonly KahlaDbContext _dbContext;
        private readonly PushKahlaMessageService _pusher;
        private readonly IConfiguration _configuration;
        private readonly AuthService<KahlaUser> _authService;
        private readonly OAuthService _oauthService;
        private readonly ServiceLocation _serviceLocation;
        private readonly ChannelService _channelService;
        private readonly StorageService _storageService;
        private readonly AppsContainer _appsContainer;
        private readonly UserService _userService;
        private readonly IHostingEnvironment _env;
        private readonly SecretService _secretService;
        private readonly object _obj = new object();

        public ApiController(
            UserManager<KahlaUser> userManager,
            SignInManager<KahlaUser> signInManager,
            KahlaDbContext dbContext,
            PushKahlaMessageService pushService,
            IConfiguration configuration,
            AuthService<KahlaUser> authService,
            OAuthService oauthService,
            ServiceLocation serviceLocation,
            ChannelService channelService,
            StorageService storageService,
            AppsContainer appsContainer,
            UserService userService,
            IHostingEnvironment env,
            SecretService secretService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _pusher = pushService;
            _configuration = configuration;
            _authService = authService;
            _serviceLocation = serviceLocation;
            _oauthService = oauthService;
            _channelService = channelService;
            _storageService = storageService;
            _appsContainer = appsContainer;
            _userService = userService;
            _env = env;
            _secretService = secretService;
        }






        [AiurForceAuth]
        public async Task<IActionResult> SearchGroup(SearchGroupAddressModel model)
        {
            var groups = await _dbContext
                .GroupConversations
                .AsNoTracking()
                .Where(t => t.GroupName.Contains(model.GroupName, StringComparison.CurrentCultureIgnoreCase))
                .Take(model.Take)
                .ToListAsync();

            return Json(new AiurCollection<GroupConversation>(groups)
            {
                Code = ErrorType.Success,
                Message = "Search result is shown."
            });
        }


        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> CreateGroupConversation(CreateGroupConversationAddressModel model)
        {
            var user = await GetKahlaUser();
            model.GroupName = model.GroupName.Trim().ToLower();
            var exsists = _dbContext.GroupConversations.Any(t => t.GroupName == model.GroupName);
            if (exsists)
            {
                return this.Protocal(ErrorType.NotEnoughResources, $"A group with name: {model.GroupName} was already exists!");
            }
            var limitedDate = DateTime.UtcNow - new TimeSpan(1, 0, 0, 0);
            var todayCreated = await _dbContext
                .GroupConversations
                .Where(t => t.OwnerId == user.Id)
                .Where(t => t.ConversationCreateTime > limitedDate)
                .CountAsync();
            if (todayCreated > 4)
            {
                return this.Protocal(ErrorType.NotEnoughResources, "You have created too many groups today. Try it tomorrow!");
            }
            var createdGroup = await _dbContext.CreateGroup(model.GroupName, user.Id);
            var newRelationship = new UserGroupRelation
            {
                UserId = user.Id,
                GroupId = createdGroup.Id,
                ReadTimeStamp = DateTime.MinValue
            };
            _dbContext.UserGroupRelations.Add(newRelationship);
            await _dbContext.SaveChangesAsync();
            return Json(new AiurValue<int>(createdGroup.Id)
            {
                Code = ErrorType.Success,
                Message = "You have successfully created a new group and joined it!"
            });
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> JoinGroup([Required]string groupName)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocal(ErrorType.NotFound, $"We can not find a group with name: {groupName}!");
            }
            var joined = _dbContext.UserGroupRelations.Any(t => t.UserId == user.Id && t.GroupId == group.Id);
            if (joined)
            {
                return this.Protocal(ErrorType.HasDoneAlready, $"You have already joined the group: {groupName}!");
            }
            // All checked and able to join him.
            // Warning: Currently we do not have invitation system for invitation control is too complicated.
            var newRelationship = new UserGroupRelation
            {
                UserId = user.Id,
                GroupId = group.Id
            };
            _dbContext.UserGroupRelations.Add(newRelationship);
            await _dbContext.SaveChangesAsync();
            return this.Protocal(ErrorType.Success, $"You have successfully joint the group: {groupName}!");
        }

        [HttpPost]
        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> LeaveGroup(string groupName)
        {
            var user = await GetKahlaUser();
            var group = await _dbContext.GroupConversations.SingleOrDefaultAsync(t => t.GroupName == groupName);
            if (group == null)
            {
                return this.Protocal(ErrorType.NotFound, $"We can not find a group with name: {groupName}!");
            }
            var joined = await _dbContext.UserGroupRelations.SingleOrDefaultAsync(t => t.UserId == user.Id && t.GroupId == group.Id);
            if (joined == null)
            {
                return this.Protocal(ErrorType.HasDoneAlready, $"You did not joined the group: {groupName} at all!");
            }
            _dbContext.UserGroupRelations.Remove(joined);
            await _dbContext.SaveChangesAsync();

            var any = _dbContext.UserGroupRelations.Any(t => t.GroupId == group.Id);
            if (!any)
            {
                _dbContext.GroupConversations.Remove(group);
                await _dbContext.SaveChangesAsync();
            }
            return this.Protocal(ErrorType.Success, $"You have successfully left the group: {groupName}!");
        }

        [AiurForceAuth(directlyReject: true)]
        public async Task<IActionResult> InitPusher()
        {
            var user = await GetKahlaUser();
            if (user.CurrentChannel == -1 || (await _channelService.ValidateChannelAsync(user.CurrentChannel, user.ConnectKey)).Code != ErrorType.Success)
            {
                var channel = await _pusher.Init(user.Id);
                user.CurrentChannel = channel.ChannelId;
                user.ConnectKey = channel.ConnectKey;
                await _userManager.UpdateAsync(user);
            }
            var model = new InitPusherViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully get your channel.",
                ChannelId = user.CurrentChannel,
                ConnectKey = user.ConnectKey,
                ServerPath = new AiurUrl(_serviceLocation.StargateListenAddress, "Listen", "Channel", new ChannelAddressModel
                {
                    Id = user.CurrentChannel,
                    Key = user.ConnectKey
                }).ToString()
            };
            return Json(model);
        }

        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return this.Protocal(ErrorType.Success, "Success.");
        }

        private async Task<KahlaUser> GetKahlaUser()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }
            return await _userManager.FindByNameAsync(User.Identity.Name);
        }
    }
}
