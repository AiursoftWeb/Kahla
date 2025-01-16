using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
// ReSharper disable InconsistentlySynchronizedField

namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Implements <see cref="IImageProcessingService"/> using ImageSharp.
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly PathResolver _pathResolver;
    private readonly FileLockProvider _fileLockProvider;

    public ImageProcessingService(
        ILogger<ImageProcessingService> logger,
        IOptions<ImageProcessingOptions> options,
        FileLockProvider fileLockProvider)
    {
        _logger = logger;
        _fileLockProvider = fileLockProvider;
        var opts = options.Value 
                   ?? throw new ArgumentNullException(nameof(options), "ImageProcessingOptions must not be null!");
        
        // Create the PathResolver with the provided config
        _pathResolver = new PathResolver(opts);

        // 如果目录不存在，可以在此处创建
        if (!Directory.Exists(opts.WorkspaceFolder))
        {
            Directory.CreateDirectory(opts.WorkspaceFolder);
        }
        if (!Directory.Exists(opts.ProcessedFolder))
        {
            Directory.CreateDirectory(opts.ProcessedFolder);
        }
    }

    /// <summary>
    /// Clears the EXIF data while retaining the same resolution, 
    /// then writes the result to the "ClearedEXIF" subdirectory.
    /// </summary>
    public string ClearExif(string relativePath)
    {
        var sourceAbsolute = _pathResolver.GetOriginalAbsolutePath(relativePath);
        var targetAbsolute = _pathResolver.GetClearedExifAbsolutePath(relativePath);

        EnsureDirectoryExists(targetAbsolute);

        // We do a lock on both source and target so that only one thread can do the same 
        // operation at a time. You can simplify or remove this if you don't need concurrency control.
        lock (_fileLockProvider.GetLock(sourceAbsolute))
        {
            lock (_fileLockProvider.GetLock(targetAbsolute))
            {
                // If it already exists, we might choose to skip re-creating it. 
                // Or we can overwrite. For demo, let's just skip if it's readable.
                if (File.Exists(targetAbsolute) && FileCanBeRead(targetAbsolute))
                {
                    _logger.LogInformation("EXIF-cleared file already exists: {Target}", targetAbsolute);
                    return _pathResolver.GetPathRelativeToProcessed(targetAbsolute);
                }

                try
                {
                    using var image = Image.Load(sourceAbsolute);
                    image.Mutate(ctx =>
                    {
                        // AutoOrient will rotate the image according to EXIF orientation 
                        // so that the final image is in a correct orientation.
                        ctx.AutoOrient();
                    });
                    // Remove EXIF
                    image.Metadata.ExifProfile = null;

                    // Save
                    _logger.LogInformation("Clearing EXIF: {Source} -> {Target}", sourceAbsolute, targetAbsolute);
                    image.SaveAsync(targetAbsolute).Wait();
                }
                catch (UnknownImageFormatException e)
                {
                    _logger.LogWarning(e, "Not a known image format; skipping EXIF clear for {Source}", sourceAbsolute);
                    // Return original. Or you can throw an exception, up to you.
                    return relativePath;
                }
                catch (ImageFormatException e)
                {
                    // e.g. if it's a corrupted or non-image file
                    _logger.LogWarning(e, "Invalid image; returning original path for {Source}", sourceAbsolute);
                    return relativePath;
                }
            }
        }

        return _pathResolver.GetPathRelativeToProcessed(targetAbsolute);
    }

    /// <summary>
    /// Compresses the image to the specified width/height. 
    /// If width or height is 0, that dimension is not constrained.
    /// Also clears EXIF data.
    /// </summary>
    public string Compress(string relativePath, int width, int height)
    {
        var sourceAbsolute = _pathResolver.GetOriginalAbsolutePath(relativePath);
        var targetAbsolute = _pathResolver.GetCompressedAbsolutePath(relativePath, width, height);

        EnsureDirectoryExists(targetAbsolute);

        lock (_fileLockProvider.GetLock(sourceAbsolute))
        {
            lock (_fileLockProvider.GetLock(targetAbsolute))
            {
                if (File.Exists(targetAbsolute) && FileCanBeRead(targetAbsolute))
                {
                    _logger.LogInformation("Compressed file already exists: {Target}", targetAbsolute);
                    return _pathResolver.GetPathRelativeToProcessed(targetAbsolute);
                }

                try
                {
                    using var image = Image.Load(sourceAbsolute);
                    image.Mutate(ctx =>
                    {
                        ctx.AutoOrient();
                        // Remove EXIF
                        // ReSharper disable once AccessToDisposedClosure
                        image.Metadata.ExifProfile = null;

                        if (width > 0 && height > 0)
                        {
                            ctx.Resize(width, height);
                        }
                        else if (width > 0)
                        {
                            // Height is auto by aspect ratio
                            ctx.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(width, int.MaxValue)
                            });
                        }
                        else if (height > 0)
                        {
                            // Width is auto by aspect ratio
                            ctx.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(int.MaxValue, height)
                            });
                        }
                    });

                    _logger.LogInformation(
                        "Compressing image {Source} -> {Target} (width={Width}, height={Height})",
                        sourceAbsolute, targetAbsolute, width, height);

                    image.Save(targetAbsolute);
                }
                catch (UnknownImageFormatException e)
                {
                    _logger.LogWarning(e, "Not a known image format; skipping compression for {Source}", sourceAbsolute);
                    // Return original or throw. For demo, we return original.
                    return relativePath;
                }
                catch (ImageFormatException e)
                {
                    _logger.LogWarning(e, "Invalid image format; returning original path for {Source}", sourceAbsolute);
                    return relativePath;
                }
            }
        }

        return _pathResolver.GetPathRelativeToProcessed(targetAbsolute);
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

    private void EnsureDirectoryExists(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
