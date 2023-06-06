using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Identity.Attributes;
using Aiursoft.Probe.SDK.Models.FilesAddressModels;
using Aiursoft.Probe.SDK.Models.FilesViewModels;
using Aiursoft.Probe.SDK.Services.ToProbeServer;
using Aiursoft.WebTools;
using Aiursoft.XelNaga.Models;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aiursoft.Directory.SDK.Services;
using Aiursoft.Probe.SDK.Configuration;
using Microsoft.Extensions.Options;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIRemoteExceptionHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class StorageController : ControllerBase
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        private readonly AppsContainer _appsContainer;
        private readonly ProbeConfiguration _probeLocator;
        private readonly FilesService _probeFileService;

        public StorageController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            IConfiguration configuration,
            TokenService tokenService,
            AppsContainer appsContainer,
            IOptions<ProbeConfiguration> probeLocator,
            FilesService probeFileService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _tokenService = tokenService;
            _appsContainer = appsContainer;
            _probeLocator = probeLocator.Value;
            _probeFileService = probeFileService;
        }

        [HttpGet]
        [Produces(typeof(AiurValue<string>))]
        public async Task<IActionResult> InitIconUpload()
        {
            var accessToken = await _appsContainer.GetAccessTokenAsync();
            var siteName = _configuration["UserIconsSiteName"];
            var path = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var token = await _tokenService.GetTokenAsync(
                accessToken,
                siteName,
                new[] { "Upload" },
                path,
                TimeSpan.FromMinutes(10));
            var address = new AiurUrl(_probeLocator.Instance, $"/Files/UploadFile/{siteName}/{path}", new UploadFileAddressModel
            {
                Token = token,
                RecursiveCreate = true
            });
            return this.Protocol(new AiurValue<string>(address.ToString())
            {
                Code = ErrorType.Success,
                Message = "Token is given. You can not upload your file to that address. And your will get your response as 'FilePath'."
            });
        }

        [HttpGet]
        [Produces(typeof(InitFileAccessViewModel))]
        public async Task<IActionResult> InitFileAccess(InitFileAccessAddressModel model)
        {
            var conversation = await _dbContext
                .Conversations
                .Include(nameof(GroupConversation.Users))
                .SingleOrDefaultAsync(t => t.Id == model.ConversationId);
            if (conversation == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Could not find the target conversation with id: {model.ConversationId}!");
            }
            var user = await GetKahlaUser();
            if (!conversation.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not authorized to upload file to conversation: {conversation.Id}!");
            }
            var accessToken = await _appsContainer.GetAccessTokenAsync();
            var siteName = _configuration["UserFilesSiteName"];
            var path = $"conversation-{conversation.Id}";
            var permissions = new List<string>();
            if (model.Upload) permissions.Add("Upload");
            if (model.Download) permissions.Add("Download");
            var token = await _tokenService.GetTokenAsync(
                accessToken,
                siteName,
                permissions.ToArray(),
                path,
                TimeSpan.FromMinutes(60));
            var address = new AiurUrl(_probeLocator.Instance, $"/Files/UploadFile/{siteName}/{path}/{DateTime.UtcNow:yyyy-MM-dd}", new UploadFileAddressModel
            {
                Token = token,
                RecursiveCreate = true
            });
            return this.Protocol(new InitFileAccessViewModel(token)
            {
                UploadAddress = address.ToString(),
                Code = ErrorType.Success,
                Message = "Token is given. You can access probe API with the token now. Permissions: " + string.Join(",", permissions)
            });
        }

        [HttpPost]
        [Produces(typeof(UploadFileViewModel))]
        public async Task<IActionResult> ForwardMedia(ForwardMediaAddressModel model)
        {
            var user = await GetKahlaUser();
            var sourceConversation = await _dbContext
                .Conversations
                .Include(nameof(GroupConversation.Users))
                .SingleOrDefaultAsync(t => t.Id == model.SourceConversationId);
            var targetConversation = await _dbContext
                .Conversations
                .Include(nameof(GroupConversation.Users))
                .SingleOrDefaultAsync(t => t.Id == model.TargetConversationId);
            if (sourceConversation == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Could not find the source conversation with id: {model.SourceConversationId}!");
            }
            if (targetConversation == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Could not find the target conversation with id: {model.TargetConversationId}!");
            }
            if (!sourceConversation.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not authorized to access file from conversation: {sourceConversation.Id}!");
            }
            if (!targetConversation.HasUser(user.Id))
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not authorized to access file from conversation: {targetConversation.Id}!");
            }
            var accessToken = await _appsContainer.GetAccessTokenAsync();
            var siteName = _configuration["UserFilesSiteName"];
            var response = await _probeFileService.CopyFileAsync(
                accessToken: accessToken,
                siteName: siteName,
                folderNames: $"conversation-{sourceConversation.Id}/{model.SourceFilePath}",
                targetSiteName: siteName,
                targetFolderNames: $"conversation-{targetConversation.Id}/{DateTime.UtcNow:yyyy-MM-dd}");
            return this.Protocol(response);
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
