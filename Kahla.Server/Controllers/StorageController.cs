using Aiursoft.Archon.SDK.Services;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Probe.SDK.Services;
using Aiursoft.Probe.SDK.Services.ToProbeServer;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.XelNaga.Models;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiAddressModels;
using Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class StorageController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        private readonly AppsContainer _appsContainer;
        private readonly ProbeLocator _probeLocator;

        public StorageController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            IConfiguration configuration,
            TokenService tokenService,
            AppsContainer appsContainer,
            ProbeLocator probeLocator)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _tokenService = tokenService;
            _appsContainer = appsContainer;
            _probeLocator = probeLocator;
        }

        [HttpGet]
        [APIProduces(typeof(AiurValue<string>))]
        public async Task<IActionResult> InitIconUpload()
        {
            var accessToken = await _appsContainer.AccessToken();
            var siteName = _configuration["UserIconsSiteName"];
            var path = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var token = await _tokenService.GetUploadTokenAsync(
                accessToken,
                siteName,
                "Upload",
                path);
            var address = new AiurUrl(_probeLocator.Endpoint, $"/Files/UploadFile/{siteName}/{path}", new
            {
                pbtoken = token,
                recursiveCreate = true
            });
            return Json(new AiurValue<string>(address.ToString())
            {
                Code = ErrorType.Success,
                Message = $"Token is given. You can not upload your file to that address. And your will get your response as 'FilePath'."
            });
        }

        [HttpGet]
        [APIProduces(typeof(AiurValue<string>))]
        public async Task<IActionResult> InitFileUpload(InitFileUpload model)
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
            var accessToken = await _appsContainer.AccessToken();
            var siteName = _configuration["UserFilesSiteName"];
            var path = $"conversation-{conversation.Id}/{DateTime.UtcNow:yyyy-MM-dd}";
            var token = await _tokenService.GetUploadTokenAsync(
                accessToken,
                siteName,
                "Upload",
                path);
            var address = new AiurUrl(_probeLocator.Endpoint, $"/Files/UploadFile/{siteName}/{path}", new
            {
                pbtoken = token,
                recursiveCreate = true
            });
            return Json(new AiurValue<string>(address.ToString())
            {
                Code = ErrorType.Success,
                Message = $"Token is given. You can not upload your file to that address. And your will get your response as 'FilePath'."
            });
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
