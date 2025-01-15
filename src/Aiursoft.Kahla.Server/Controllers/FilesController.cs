using System.Collections.Concurrent;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.CSTools.Attributes;
using Aiursoft.CSTools.Tools;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Aiursoft.Kahla.Server.Controllers;

/// <summary>
/// Represents a service for storing and retrieving files.
/// </summary>
public class StorageService(IConfiguration configuration)
{
    private readonly string _workspaceFolder = Path.Combine(configuration["Storage:Path"]!, "Workspace");

    // Async lock.
    private readonly SemaphoreSlim _uniqueFileCreationLock = new(1, 1);

    /// <summary>
    /// Saves a file to the storage.
    /// </summary>
    /// <param name="saveRelativePath">The path where the file will be saved. The 'savePath' is the path that the user wants to save. Not related to actual disk path.</param>
    /// <param name="file">The file to be saved.</param>
    /// <returns>The actual path where the file is saved relative to the workspace folder.</returns>
    public async Task<string> Save(string saveRelativePath, IFormFile file)
    {
        var finalFilePath = Path.Combine(_workspaceFolder, saveRelativePath);
        var finalFolder = Path.GetDirectoryName(finalFilePath);

        // Create the folder if it does not exist.
        if (!Directory.Exists(finalFolder))
        {
            Directory.CreateDirectory(finalFolder!);
        }

        // The problem is: What if the file already exists?
        await _uniqueFileCreationLock.WaitAsync();
        try
        {
            var expectedFileName = Path.GetFileName(finalFilePath);
            while (File.Exists(finalFilePath))
            {
                expectedFileName = "_" + expectedFileName;
                finalFilePath = Path.Combine(finalFolder!, expectedFileName);
            }

            // Create a new file.
            File.Create(finalFilePath).Close();
        }
        finally
        {
            _uniqueFileCreationLock.Release();
        }

        await using var fileStream = new FileStream(finalFilePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        fileStream.Close();
        return Path.GetRelativePath(_workspaceFolder, finalFilePath);
    }

    public string GetFilePhysicalPath(string fileName)
    {
        return Path.Combine(_workspaceFolder, fileName);
    }

    public string RelativePathToUriPath(string relativePath)
    {
        var urlPath = Uri.EscapeDataString(relativePath)
            .Replace("%5C", "/")
            .Replace("%5c", "/")
            .Replace("%2F", "/")
            .Replace("%2f", "/")
            .TrimStart('/');
        return urlPath;
    }
}

[LimitPerMin]
[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/files")]
public class FilesController(
    ImageCompressor imageCompressor,
    ILogger<FilesController> logger,
    StorageService storage,
    ThreadsInMemoryCache threadCache,
    KahlaRelationalDbContext relationalDbContext)
    : ControllerBase
{
    // TODO: This should be migrated to a service.
    private void EnsureUserCanRead(int threadId)
    {
        var userId = User.GetUserId();
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
            throw new AiurServerException(Code.Unauthorized, "You are not allowed to send messages in this thread.");
        }
    }

    [Route("Upload/{ThreadId:int}")]
    public async Task<IActionResult> Upload(int threadId)
    {
        await EnsureUserCanUpload(threadId);
        
        // Executing here will let the browser upload the file.
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

        return Ok(new
        {
            // Krl is a safe resource path for Kahla. Client side should use Krl instead of the InternetOpenPath to avoid hackers injecting malicious files to messages.
            Krl = $"kahla://server/{threadId}/{uriPath}",
            InternetOpenPath = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/files/Open/{threadId}/{uriPath}",
            InternetDownloadPath = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/files/File/{threadId}/{uriPath}",
        });
    }

    [Route("File/{ThreadId}/{**FolderNames}", Name = "File")]
    [Route("Open/{ThreadId}/{**FolderNames}", Name = "Open")]
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

        if (ControllerContext.ActionDescriptor.AttributeRouteInfo?.Name == "File")
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
            logger.LogTrace("File with path {ImagePath} is an valid image", imagePath);
            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "File with path {ImagePath} is not an valid image", imagePath);
            return false;
        }
    }

    private async Task<IActionResult> FileWithImageCompressor(string path, string extension)
    {
        var passedWidth = int.TryParse(Request.Query["w"], out var width);
        var passedSquare = bool.TryParse(Request.Query["square"], out var square);
        if (width > 0 && passedWidth)
        {
            if (square && passedSquare)
            {
                return this.WebFile(await imageCompressor.Compress(path, width, width, extension), extension);
            }

            return this.WebFile(await imageCompressor.Compress(path, width, 0, extension), extension);
        }

        return this.WebFile(await imageCompressor.ClearExif(path), extension);
    }
}

public class ImageCompressor(
    ILogger<ImageCompressor> logger,
    IConfiguration configuration)
{
    private static readonly ConcurrentDictionary<string, object> ReadFileLockMapping = new();
    private static readonly ConcurrentDictionary<string, object> WriteFileLockMapping = new();
    private readonly string _compressorFolder = Path.Combine(configuration["Storage:Path"]!, "ImageCompressor");

    private static object GetFileReadLock(string path)
    {
        if (ReadFileLockMapping.TryGetValue(path, out var mapping))
        {
            return mapping;
        }

        ReadFileLockMapping.TryAdd(path, new object());
        return GetFileReadLock(path);
    }

    private static object GetFileWriteLock(string path)
    {
        if (WriteFileLockMapping.TryGetValue(path, out var mapping))
        {
            return mapping;
        }

        WriteFileLockMapping.TryAdd(path, new object());
        return GetFileReadLock(path);
    }

    public async Task<string> ClearExif(string inputFile)
    {
        try
        {
            var clearedFolder = Path.Combine(_compressorFolder, "ClearedEXIF");
            if (Directory.Exists(clearedFolder) == false)
            {
                Directory.CreateDirectory(clearedFolder);
            }

            var clearedImagePath = $"{clearedFolder}probe_cleared_file_id_{Path.GetFileNameWithoutExtension(inputFile)}.dat";
            await ClearImage(inputFile, clearedImagePath);
            return clearedImagePath;
        }
        catch (ImageFormatException ex)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            logger.LogError(ex, "Failed to clear the EXIF of an image. Have to return the original image.");
            return inputFile;
        }
    }

    private async Task ClearImage(string sourceImage, string saveTarget)
    {
        var fileExists = File.Exists(saveTarget);
        var fileCanRead = FileCanBeRead(saveTarget);
        if (fileExists && fileCanRead)
        {
            // Return. Because we have already cleared it.
            return;
        }

        if (fileExists)
        {
            while (!FileCanBeRead(saveTarget))
            {
                // Wait till the file is readable.
                await Task.Delay(500);
            }
        }
        else
        {
            lock (GetFileReadLock(sourceImage))
            {
                lock (GetFileWriteLock(saveTarget))
                {
                    logger.LogInformation("Trying to clear EXIF for image {Source} and save to {Target}", sourceImage,
                        saveTarget);
                    var image = Image.Load(sourceImage);
                    image.Mutate(x => x.AutoOrient());
                    image.Metadata.ExifProfile = null;
                    image.Save(saveTarget ??
                               throw new NullReferenceException(
                                   $"When compressing image, {nameof(saveTarget)} is null!"));
                }
            }
        }
    }

    public async Task<string> Compress(string path, int width, int height, string extension)
    {
        width = SizeCalculator.Ceiling(width);
        height = SizeCalculator.Ceiling(height);
        try
        {
            var compressedFolder = Path.Combine(_compressorFolder, "Compressed");
            if (Directory.Exists(compressedFolder) == false)
            {
                Directory.CreateDirectory(compressedFolder);
            }

            var compressedImagePath =
                $"{compressedFolder}probe_compressed_w{width}_h{height}_fileId_{Path.GetFileNameWithoutExtension(path)}.{extension}";
            await SaveCompressedImage(path, compressedImagePath, width, height);
            return compressedImagePath;
        }
        catch (ImageFormatException ex)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            logger.LogError(ex, "Failed to compress an image");
            return path;
        }
    }

    private async Task SaveCompressedImage(string sourceImage, string saveTarget, int width, int height)
    {
        var fileExists = File.Exists(saveTarget);
        var fileCanRead = FileCanBeRead(saveTarget);
        // Return. Because we have already cleared it.
        if (fileExists && fileCanRead)
        {
            return;
        }

        if (fileExists)
        {
            while (!FileCanBeRead(saveTarget))
            {
                await Task.Delay(500);
            }
        }
        else
        {
            lock (GetFileReadLock(sourceImage))
            {
                lock (GetFileWriteLock(saveTarget))
                {
                    logger.LogInformation("Trying to compress for image {Source} and save to {Target}", sourceImage,
                        saveTarget);
                    var image = Image.Load(sourceImage);
                    image.Mutate(x => x.AutoOrient());
                    image.Metadata.ExifProfile = null;
                    image.Mutate(x => x.Resize(width, height));
                    image.Save(saveTarget ??
                               throw new NullReferenceException(
                                   $"When compressing image, {nameof(saveTarget)} is null!"));
                }
            }
        }
    }

    private bool FileCanBeRead(string filepath)
    {
        try
        {
            File.Open(filepath, FileMode.Open, FileAccess.Read).Dispose();
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}

/// <summary>
/// Provides functionality to calculate size based on powers of two.
/// </summary>
/// <remarks>
/// This class is used to compute size values by finding the smallest
/// power of two that is greater than or equal to the input.
/// </remarks>
public class SizeCalculator
{
    private static IEnumerable<int> GetTwoPowers()
    {
        yield return 0;

        // 16384
        for (var i = 1; i <= 0x4000; i *= 2)
        {
            yield return i;
        }
    }

    /// <summary>
    /// Calculates the smallest power of two that is greater than or equal to the specified input value.
    /// </summary>
    /// <param name="input">The input integer for which the smallest greater or equal power of two is to be calculated. Must be less than or equal to 16384.</param>
    /// <returns>The smallest power of two that is greater than or equal to the input value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the calculation fails due to unexpected conditions.</exception>
    public static int Ceiling(int input)
    {
        if (input >= 0x4000)
        {
            return 0x4000;
        }

        foreach (var optional in GetTwoPowers())
        {
            if (optional >= input)
            {
                return optional;
            }
        }

        // Logic shall not reach here.
        throw new InvalidOperationException($"Image size calculation failed with input: {input}.");
    }
}