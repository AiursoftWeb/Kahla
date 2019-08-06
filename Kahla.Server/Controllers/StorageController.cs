using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToProbeServer;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
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
        private readonly StorageService _storageService;
        private readonly AppsContainer _appsContainer;
        private readonly FoldersService _folderService;

        public StorageController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            IConfiguration configuration,
            StorageService storageService,
            AppsContainer appsContainer,
            FoldersService folderService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _storageService = storageService;
            _appsContainer = appsContainer;
            _folderService = folderService;
        }

        [HttpPost]
        [FileChecker(MaxSize = 5 * 1024 * 1024)]
        [APIModelStateChecker]
        [APIProduces(typeof(UploadImageViewModel))]
        public async Task<IActionResult> UploadIcon()
        {
            var file = Request.Form.Files.First();
            if (!file.FileName.IsStaticImage())
            {
                return this.Protocol(ErrorType.InvalidInput, "The file you uploaded was not an acceptable Image. Please send a file ends with `jpg`,`png`, or `bmp`.");
            }
            var savedFile = await _storageService.SaveToProbe(file, _configuration["UserIconsSiteName"], $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}");
            return Json(new UploadImageViewModel
            {
                Code = ErrorType.Success,
                Message = $"Successfully uploaded your user icon, but we did not update your profile. Now you can call `/auth/{nameof(AuthController.UpdateInfo)}` to update your user icon.",
                FilePath = $"{savedFile.SiteName}/{savedFile.FilePath}"
            });
        }

        /// <summary>
        /// Used to upload images, videos and files.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [FileChecker]
        [APIModelStateChecker]
        [APIProduces(typeof(UploadFileViewModel))]
        public async Task<IActionResult> UploadFile(UploadFileAddressModel model)
        {
            var conversation = await _dbContext.Conversations.SingleOrDefaultAsync(t => t.Id == model.ConversationId);
            if (conversation == null)
            {
                return this.Protocol(ErrorType.NotFound, $"Could not find the target conversation with id: {model.ConversationId}!");
            }
            var user = await GetKahlaUser();
            if (!await _dbContext.VerifyJoined(user.Id, conversation))
            {
                return this.Protocol(ErrorType.Unauthorized, $"You are not authorized to upload file to conversation: {conversation.Id}!");
            }
            var file = Request.Form.Files.First();
            var path = $"conversation-{conversation.Id}/{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            var savedFile = await _storageService.SaveToProbe(file, _configuration["UserFilesSiteName"], path, SaveFileOptions.SourceName);

            return Json(new UploadFileViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully uploaded your file!",
                FilePath = $"{savedFile.SiteName}/{savedFile.FilePath}",
                FileSize = file.Length
            });
        }

        private Task<KahlaUser> GetKahlaUser() => _userManager.GetUserAsync(User);
    }
}
