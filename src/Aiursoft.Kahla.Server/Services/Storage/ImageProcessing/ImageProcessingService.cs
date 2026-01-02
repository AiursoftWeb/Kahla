using AsyncKeyedLock;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Implements <see cref="IImageProcessingService"/> using ImageSharp.
/// </summary>
public class ImageProcessingService(
    PathResolver pathResolver,
    ILogger<ImageProcessingService> logger,
    AsyncKeyedLocker<string> fileLockProvider)
    : IImageProcessingService
{
    /// <summary>
    /// Clears the EXIF data while retaining the same resolution, 
    /// then writes the result to the "ClearedEXIF" subdirectory.
    /// </summary>
    public async Task<string> ClearExifAsync(string sourceAbsolute)
    {
        var targetAbsolute = pathResolver.GetClearedExifAbsolutePath(sourceAbsolute);
        if (File.Exists(targetAbsolute) && FileCanBeRead(targetAbsolute))
        {
            logger.LogInformation("EXIF-cleared file already exists: {Target}", targetAbsolute);
            return targetAbsolute;
        }

        using var _ = await fileLockProvider.LockAsync(targetAbsolute).ConfigureAwait(false);
        try
        {
            await WaitTillFileCanBeReadAsync(sourceAbsolute);
            using var image = await Image.LoadAsync(sourceAbsolute);
            image.Mutate(ctx => { ctx.AutoOrient(); });
            image.Metadata.ExifProfile = null;
            logger.LogInformation("Clearing EXIF: {Source} -> {Target}", sourceAbsolute, targetAbsolute);
            await image.SaveAsync(targetAbsolute);
            return targetAbsolute;
        }
        catch (UnknownImageFormatException e)
        {
            logger.LogWarning(e, "Not a known image format; skipping EXIF clear for {Source}", sourceAbsolute);
            // Return original. Or you can throw an exception, up to you.
            return sourceAbsolute;
        }
        catch (ImageFormatException e)
        {
            // e.g. if it's a corrupted or non-image file
            logger.LogWarning(e, "Invalid image; returning original path for {Source}", sourceAbsolute);
            return sourceAbsolute;
        }        
    }

    /// <summary>
    /// Compresses the image to the specified width/height. 
    /// If width or height is 0, that dimension is not constrained.
    /// Also clears EXIF data.
    /// </summary>
    public async Task<string> CompressAsync(string sourceAbsolute, int width, int height)
    {
        var targetAbsolute = pathResolver.GetCompressedAbsolutePath(sourceAbsolute, width, height);
        if (File.Exists(targetAbsolute) && FileCanBeRead(targetAbsolute))
        {
            logger.LogInformation("Compressed file already exists: {Target}", targetAbsolute);
            return targetAbsolute;
        }

        using var _ = await fileLockProvider.LockAsync(targetAbsolute).ConfigureAwait(false);
        try
        {
            await WaitTillFileCanBeReadAsync(sourceAbsolute);
            using var image = await Image.LoadAsync(sourceAbsolute);
            image.Mutate(x => x.AutoOrient());
            image.Metadata.ExifProfile = null;
            image.Mutate(x => x.Resize(width, height));
            logger.LogInformation("Compressing image {Source} -> {Target} (width={Width}, height={Height})",
                sourceAbsolute, targetAbsolute, width, height);
            await image.SaveAsync(targetAbsolute);
            return targetAbsolute;
        }
        catch (UnknownImageFormatException e)
        {
            logger.LogWarning(e, "Not a known image format; skipping compression for {Source}", sourceAbsolute);
            return sourceAbsolute;
        }
        catch (ImageFormatException e)
        {
            logger.LogWarning(e, "Invalid image format; returning original path for {Source}", sourceAbsolute);
            return sourceAbsolute;
        }
    }

    private async Task WaitTillFileCanBeReadAsync(string path)
    {
        while (!FileCanBeRead(path))
        {
            await Task.Delay(100);
        }
    }
    
    private bool FileCanBeRead(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
