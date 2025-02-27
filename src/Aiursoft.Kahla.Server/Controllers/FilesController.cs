using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.CSTools.Attributes;
using Aiursoft.CSTools.Tools;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.Entities;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Storage;
using Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/files")]
public class FilesController(
    IImageProcessingService imageCompressor,
    ILogger<FilesController> logger,
    StorageService storage,
    QuickMessageAccess quickMessageAccess,
    KahlaRelationalDbContext relationalDbContext)
    : ControllerBase
{
    // TODO: This should be migrated to a service.
    private void EnsureUserCanRead(int threadId)
    {
        var userId = User.GetUserId();
        var threadCache = quickMessageAccess.GetThreadCache(threadId);
        if (!threadCache.IsUserInThread(userId))
        {
            logger.LogWarning(
                "User with ID: {UserId} is trying to get a websocket for thread {ThreadId} that he is not in.",
                userId, threadId);
            throw new AiurServerException(Code.Unauthorized, "You are not a member of this thread.");
        }
    }

    // TODO: This should be migrated to a service.
    private async Task EnsureUserCanUpload(int threadId)
    {
        var userId = User.GetUserId();
        var thread = await relationalDbContext.ChatThreads.FindAsync(threadId);
        if (thread == null)
        {
            logger.LogWarning(
                "User with ID {UserId} is trying to upload files to a thread that does not exist: {ThreadId}.",
                userId, threadId);
            throw new AiurServerException(Code.NotFound, "The thread does not exist.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == userId)
            .Where(t => t.ThreadId == threadId)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            logger.LogWarning(
                "User with ID {UserId} is trying to upload files to thread {ThreadId} that he is not a member of.",
                userId, threadId);
            throw new AiurServerException(Code.Unauthorized, "You are not a member of this thread.");
        }

        // Not allowing sending messages, and I'm not an admin.
        if (!thread.AllowMembersSendMessages && myRelation.UserThreadRole != UserThreadRole.Admin)
        {
            logger.LogWarning(
                "User with ID {UserId} is trying to send messages in thread {ThreadId} that he is not allowed to send messages in.",
                userId, threadId);
            throw new AiurServerException(Code.Unauthorized,
                "You are not allowed to send messages in this thread.");
        }
    }

    [Route("Upload/{ThreadId:int}")]
    [HttpPost]
    public async Task<IActionResult> Upload(int threadId)
    {
        await EnsureUserCanUpload(threadId);

        try
        {
            _ = HttpContext.Request.Form.Files.FirstOrDefault()?.ContentType;
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }

        if (HttpContext.Request.Form.Files.Count < 1)
        {
            return BadRequest("No file uploaded!");
        }

        var file = HttpContext.Request.Form.Files.First();
        if (!new ValidFolderName().IsValid(file.FileName))
        {
            return BadRequest($"Invalid file name '{file.FileName}'!");
        }

        var storePath = Path.Combine(
            "thread-files",
            threadId.ToString(),
            DateTime.UtcNow.Year.ToString("D4"),
            DateTime.UtcNow.Month.ToString("D2"),
            DateTime.UtcNow.Day.ToString("D2"),
            file.FileName);

        var relativePath = await storage.Save(storePath, file);
        var uriPath = storage.RelativePathToUriPath(relativePath);

        return this.Protocol(new UploadViewModel
        {
            Code = Code.JobDone,
            Message =
                $"File uploaded! Please use the Krl to save in messages. To convert a Krl to a real URL, please append the path after /server/ as {Request.Scheme}://{Request.Host}/api/files/open/*.",
            Krl = $"kahla://server/{uriPath}",
            InternetOpenPath = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/files/open/{uriPath}",
            InternetDownloadPath =
                $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/files/download/{uriPath}",
        });
    }

    [Route("Download/thread-files/{ThreadId}/{**FolderNames}", Name = "File")]
    [Route("Open/thread-files/{ThreadId}/{**FolderNames}", Name = "Open")]
    public async Task<IActionResult> Open(OpenAddressModel model)
    {
        EnsureUserCanRead(model.ThreadId);

        var relativePath = Path.Combine(
            "thread-files",
            model.ThreadId.ToString(),
            model.FolderNames);

        var path = storage.GetFilePhysicalPath(relativePath);

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var fileName = Path.GetFileName(path);
        var extension = Path.GetExtension(path).TrimStart('.');

        if (ControllerContext.ActionDescriptor.AttributeRouteInfo?.Name == "Download")
        {
            return this.WebFile(path, "do-not-open");
        }

        if (fileName.IsStaticImage() && await IsValidImageAsync(path))
        {
            return await FileWithImageCompressor(path, extension);
        }

        return this.WebFile(path, extension);
    }

    private async Task<bool> IsValidImageAsync(string imagePath)
    {
        try
        {
            _ = await Image.DetectFormatAsync(imagePath);
            logger.LogTrace("File with path {ImagePath} is a valid image", imagePath);
            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "File with path {ImagePath} is not a valid image", imagePath);
            return false;
        }
    }

    private async Task<IActionResult> FileWithImageCompressor(string path, string extension)
    {
        var passedWidth = int.TryParse(Request.Query["w"], out var width);
        var passedSquare = bool.TryParse(Request.Query["square"], out var square);
        if (width > 0 && passedWidth)
        {
            width = SizeCalculator.Ceiling(width);
            if (square && passedSquare)
            {
                var compressedPath = await imageCompressor.CompressAsync(path, width, width);
                return this.WebFile(compressedPath, extension);
            }
            else
            {
                var compressedPath = await imageCompressor.CompressAsync(path, width, 0);
                return this.WebFile(compressedPath, extension);
            }
        }

        // If no width or invalid, just clear EXIF
        var clearedPath = await imageCompressor.ClearExifAsync(path);
        return this.WebFile(clearedPath, extension);
    }
}