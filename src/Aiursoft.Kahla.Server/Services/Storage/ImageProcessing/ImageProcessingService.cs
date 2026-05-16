using SkiaSharp;

namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Implements <see cref="IImageProcessingService"/> using ImageSharp.
/// </summary>
public class ImageProcessingService(
    PathResolver pathResolver,
    ILogger<ImageProcessingService> logger,
    FileLockProvider fileLockProvider)
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

        var lockOnCreatedFile = fileLockProvider.GetLock(targetAbsolute);
        await lockOnCreatedFile.WaitAsync();
        try
        {
            await WaitTillFileCanBeReadAsync(sourceAbsolute);
            using var image = SKBitmap.Decode(sourceAbsolute);
            if (image == null)
            {
                logger.LogWarning("Unable to decode image; returning original path for {Source}", sourceAbsolute);
                return sourceAbsolute;
            }

            logger.LogInformation("Clearing EXIF: {Source} -> {Target}", sourceAbsolute, targetAbsolute);
            SaveImage(image, targetAbsolute);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to process image; returning original path for {Source}", sourceAbsolute);
            return sourceAbsolute;
        }
        finally
        {
            lockOnCreatedFile.Release();
        }

        return targetAbsolute;
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

        var lockOnCreatedFile = fileLockProvider.GetLock(targetAbsolute);
        await lockOnCreatedFile.WaitAsync();
        try
        {
            await WaitTillFileCanBeReadAsync(sourceAbsolute);
            using var image = SKBitmap.Decode(sourceAbsolute);
            if (image == null)
            {
                logger.LogWarning("Unable to decode image; returning original path for {Source}", sourceAbsolute);
                return sourceAbsolute;
            }

            var (newWidth, newHeight) = CalculateDimensions(image.Width, image.Height, width, height);
            using var resized = image.Resize(new SKSizeI(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear));
            if (resized == null)
            {
                logger.LogWarning("Unable to resize image; returning original path for {Source}", sourceAbsolute);
                return sourceAbsolute;
            }

            logger.LogInformation("Compressing image {Source} -> {Target} (width={Width}, height={Height})",
                sourceAbsolute, targetAbsolute, newWidth, newHeight);
            SaveImage(resized, targetAbsolute);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to process image; returning original path for {Source}", sourceAbsolute);
            return sourceAbsolute;
        }
        finally
        {
            lockOnCreatedFile.Release();
        }

        return targetAbsolute;
    }

    private static (int width, int height) CalculateDimensions(int srcWidth, int srcHeight, int targetWidth, int targetHeight)
    {
        if (targetWidth > 0 && targetHeight > 0)
            return (targetWidth, targetHeight);

        if (targetWidth > 0)
            return (targetWidth, (int)(targetWidth / (float)srcWidth * srcHeight));

        if (targetHeight > 0)
            return ((int)(targetHeight / (float)srcHeight * srcWidth), targetHeight);

        return (srcWidth, srcHeight);
    }

    private static void SaveImage(SKBitmap image, string path)
    {
        var format = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => SKEncodedImageFormat.Png,
            ".webp" => SKEncodedImageFormat.Webp,
            ".gif" => SKEncodedImageFormat.Gif,
            ".bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Jpeg
        };

        using var encoded = image.Encode(format, 85);
        using var stream = File.OpenWrite(path);
        encoded.SaveTo(stream);
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