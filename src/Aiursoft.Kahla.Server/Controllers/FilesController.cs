using System.Collections.Concurrent;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Scanner.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Aiursoft.Kahla.Server.Controllers;

public class FilesController(
    ImageCompressor imageCompressor,
    ILogger<FilesController> logger)
    : ControllerBase
{
    [Route("File/{ThreadId}/{**FolderNames}", Name = "File")]
    [Route("Open/{ThreadId}/{**FolderNames}", Name = "Open")]
    public async Task<IActionResult> Open(OpenAddressModel model)
    {
        // Ensure he joined this thread.
        // If not, return Unauthorized.

        var (folders, fileName) = _folderSplitter.SplitToFoldersAndFile(model.FolderNames);
        try
        {
            var file = await _fileRepo.GetFileInFolder(folder, fileName);
            if (file == null)
            {
                return NotFound();
            }

            var path = _storageProvider.GetFilePath(file.HardwareId);
            var extension = _storageProvider.GetExtension(file.FileName);
            if (ControllerContext.ActionDescriptor.AttributeRouteInfo?.Name == "File")
            {
                return this.WebFile(path, "do-not-open");
            }

            if (file.FileName.IsStaticImage() && await IsValidImageAsync(path))
            {
                return await FileWithImageCompressor(path, extension);
            }

            return this.WebFile(path, extension);
        }
        catch (AiurUnexpectedServerResponseException e) when (e.Response.Code == Code.NotFound)
        {
            return NotFound();
        }
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
        var passedWidth= int.TryParse(Request.Query["w"], out var width);
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


public class ImageCompressor
{
    private readonly ILogger<ImageCompressor> _logger;
    private readonly string _tempFilePath;
    private static readonly ConcurrentDictionary<string, object> ReadFileLockMapping = new();
    private static readonly ConcurrentDictionary<string, object> WriteFileLockMapping = new();

    public ImageCompressor(
        ILogger<ImageCompressor> logger,
        IOptions<DiskAccessConfig> diskAccessConfig)
    {
        _logger = logger;
        _tempFilePath = diskAccessConfig.Value.TempFileStoragePath;
        if (string.IsNullOrWhiteSpace(_tempFilePath))
        {
            _tempFilePath = diskAccessConfig.Value.StoragePath;
        }
    }
    
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

    public async Task<string> ClearExif(string path)
    {
        try
        {
            var clearedFolder =
                _tempFilePath + $"{Path.DirectorySeparatorChar}ClearedEXIF{Path.DirectorySeparatorChar}";
            if (Directory.Exists(clearedFolder) == false)
            {
                Directory.CreateDirectory(clearedFolder);
            }

            var clearedImagePath = $"{clearedFolder}probe_cleared_file_id_{Path.GetFileNameWithoutExtension(path)}.dat";
            await ClearImage(path, clearedImagePath);
            return clearedImagePath;
        }
        catch (ImageFormatException ex)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _logger.LogError(ex, "Failed to clear the EXIF of an image. Have to return the original image.");
            return path;
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
                    _logger.LogInformation("Trying to clear EXIF for image {Source} and save to {Target}", sourceImage, saveTarget);
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
            var compressedFolder =
                _tempFilePath + $"{Path.DirectorySeparatorChar}Compressed{Path.DirectorySeparatorChar}";
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
            _logger.LogError(ex, "Failed to compress an image");
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
                    _logger.LogInformation("Trying to compress for image {Source} and save to {Target}", sourceImage, saveTarget);
                    var image = Image.Load(sourceImage);
                    image.Mutate(x => x.AutoOrient());
                    image.Metadata.ExifProfile = null;
                    image.Mutate(x => x.Resize(width, height));
                    image.Save(saveTarget ?? throw new NullReferenceException($"When compressing image, {nameof(saveTarget)} is null!"));
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