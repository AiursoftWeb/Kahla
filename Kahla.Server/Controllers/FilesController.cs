using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToOSSServer;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiAddressModels;
using Kahla.Server.Models.ApiViewModels;
using Kahla.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Controllers
{
    [APIExpHandler]
    [APIModelStateChecker]
    [AiurForceAuth(directlyReject: true)]
    public class FilesController : Controller
    {
        private readonly UserManager<KahlaUser> _userManager;
        private readonly KahlaDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ServiceLocation _serviceLocation;
        private readonly StorageService _storageService;
        private readonly AppsContainer _appsContainer;
        private readonly SecretService _secretService;

        public FilesController(
            UserManager<KahlaUser> userManager,
            KahlaDbContext dbContext,
            IConfiguration configuration,
            ServiceLocation serviceLocation,
            StorageService storageService,
            AppsContainer appsContainer,
            SecretService secretService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _serviceLocation = serviceLocation;
            _storageService = storageService;
            _appsContainer = appsContainer;
            _secretService = secretService;
        }

        [HttpPost]
        [FileChecker(MaxSize = 5 * 1024 * 1024)]
        [APIModelStateChecker]
        public async Task<IActionResult> UploadIcon()
        {
            var file = Request.Form.Files.First();
            if (!file.FileName.IsStaticImage())
            {
                return this.Protocal(ErrorType.InvalidInput, "The file you uploaded was not an acceptable Image. Please send a file ends with `jpg`,`png`, or `bmp`.");
            }
            var uploadedFile = await _storageService.SaveToOSS(file, Convert.ToInt32(_configuration["KahlaUserIconsBucketId"]), 365, SaveFileOptions.RandomName);
            return this.AiurJson(new UploadImageViewModel
            {
                Code = ErrorType.Success,
                Message = $"Successfully uploaded your user icon, but we did not update your profile. Now you can call `/auth/{nameof(AuthController.UpdateInfo)}` to update your user icon.",
                FileKey = uploadedFile.FileKey,
                DownloadPath = $"{_serviceLocation.OSS}/Download/FromKey/{uploadedFile.FileKey}"
            });
        }

        /// <summary>
        /// Used to upload images and videos.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [FileChecker]
        [APIModelStateChecker]
        public async Task<IActionResult> UploadMedia()
        {
            var file = Request.Form.Files.First();
            if (!file.FileName.IsImageMedia() && !file.FileName.IsVideo())
            {
                return this.Protocal(ErrorType.InvalidInput, "The file you uploaded was not an acceptable Image nor an acceptable video. Please send a file ends with `jpg`,`png`, `bmp`, `mp4`, `ogg` or `webm`.");
            }
            var uploadedFile = await _storageService.SaveToOSS(file, Convert.ToInt32(_configuration["KahlaPublicBucketId"]), 30, SaveFileOptions.RandomName);
            return this.AiurJson(new UploadImageViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully uploaded your media file!",
                FileKey = uploadedFile.FileKey,
                DownloadPath = $"{_serviceLocation.OSS}/Download/FromKey/{uploadedFile.FileKey}"
            });
        }

        [HttpPost]
        [FileChecker]
        [APIModelStateChecker]
        public async Task<IActionResult> UploadFile(UploadFileAddressModel model)
        {
            var conversation = await _dbContext.Conversations.SingleOrDefaultAsync(t => t.Id == model.ConversationId);
            if (conversation == null)
            {
                return this.Protocal(ErrorType.NotFound, $"Could not find the target conversation with id: {model.ConversationId}!");
            }
            var user = await GetKahlaUser();
            if (!await _dbContext.VerifyJoined(user.Id, conversation))
            {
                return this.Protocal(ErrorType.Unauthorized, $"You are not authorized to upload file to conversation: {conversation.Id}!");
            }
            var file = Request.Form.Files.First();
            var uploadedFile = await _storageService.SaveToOSS(file, Convert.ToInt32(_configuration["KahlaSecretBucketId"]), 20, SaveFileOptions.RandomName);
            var fileRecord = new FileRecord
            {
                FileKey = uploadedFile.FileKey,
                SourceName = Path.GetFileName(file.FileName.Replace(" ", "")),
                UploaderId = user.Id,
                ConversationId = conversation.Id
            };
            _dbContext.FileRecords.Add(fileRecord);
            await _dbContext.SaveChangesAsync();
            return this.AiurJson(new UploadFileViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully uploaded your file!",
                FileKey = uploadedFile.FileKey,
                SavedFileName = fileRecord.SourceName,
                FileSize = file.Length
            });
        }

        [HttpPost]
        public async Task<IActionResult> FileDownloadAddress(FileDownloadAddressAddressModel model)
        {
            var record = await _dbContext
                .FileRecords
                .Include(t => t.Conversation)
                .SingleOrDefaultAsync(t => t.FileKey == model.FileKey);
            if (record == null || record.Conversation == null)
            {
                return this.Protocal(ErrorType.NotFound, "Could not find your file!");
            }
            var user = await GetKahlaUser();
            if (!await _dbContext.VerifyJoined(user.Id, record.Conversation))
            {
                return this.Protocal(ErrorType.Unauthorized, $"You are not authorized to download file from conversation: {record.Conversation.Id}!");
            }
            var secret = await _secretService.GenerateAsync(record.FileKey, await _appsContainer.AccessToken());
            return this.AiurJson(new FileDownloadAddressViewModel
            {
                Code = ErrorType.Success,
                Message = "Successfully generated your file download address!",
                FileName = record.SourceName,
                DownloadPath = $"{_serviceLocation.OSS}/Download/FromSecret?Sec={secret.Value}&sd=true&name={record.SourceName}"
            });
        }

        private Task<KahlaUser> GetKahlaUser()
        {
            return _userManager.GetUserAsync(User);
        }
    }
}
