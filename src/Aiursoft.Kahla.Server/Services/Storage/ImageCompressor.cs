using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Aiursoft.Kahla.Server.Services.Storage;

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