namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Responsible for determining the correct physical paths for 
/// reading original files and writing processed files.
/// </summary>
public class PathResolver(ImageProcessingOptions options)
{
    /// <summary>
    /// Returns the absolute path for a file that is relative to the workspace.
    /// For example, if relativePath is "thread-files/1/2025/01/15/foo.jpg",
    /// this might return "kahla-data/Workspace/thread-files/1/2025/01/15/foo.jpg".
    /// </summary>
    public string GetOriginalAbsolutePath(string relativePath)
    {
        return Path.Combine(options.WorkspaceFolder, relativePath);
    }

    /// <summary>
    /// Returns the absolute path for a file that should be placed in the "ClearedEXIF" subfolder.
    /// This retains the subfolder structure, but appends "_cleared" before the extension.
    /// 
    /// E.g. input: "thread-files/1/2025/01/15/foo.jpg"
    /// output: "kahla-data/ImageCompressor/ClearedEXIF/thread-files/1/2025/01/15/foo_cleared.jpg"
    /// </summary>
    public string GetClearedExifAbsolutePath(string relativePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(relativePath);
        var extension = Path.GetExtension(relativePath);
        var folder = Path.GetDirectoryName(relativePath) ?? string.Empty;

        var targetFolder = Path.Combine(options.ProcessedFolder, "ClearedEXIF", folder);
        var targetFileName = $"{fileName}_cleared{extension}";
        return Path.Combine(targetFolder, targetFileName);
    }

    /// <summary>
    /// Returns the absolute path for a compressed version of the image, 
    /// retaining the folder structure but adding dimension suffix.
    /// 
    /// E.g. if relativePath is "thread-files/1/2025/01/15/foo.jpg",
    /// width=100, height=0 => "kahla-data/ImageCompressor/Compressed/thread-files/1/2025/01/15/foo_w100.jpg"
    /// </summary>
    public string GetCompressedAbsolutePath(string relativePath, int width, int height)
    {
        var fileName = Path.GetFileNameWithoutExtension(relativePath);
        var extension = Path.GetExtension(relativePath);
        var folder = Path.GetDirectoryName(relativePath) ?? string.Empty;

        var dimensionSuffix = BuildDimensionSuffix(width, height);
        var targetFolder = Path.Combine(options.ProcessedFolder, "Compressed", folder);
        var targetFileName = $"{fileName}{dimensionSuffix}{extension}";
        return Path.Combine(targetFolder, targetFileName);
    }

    /// <summary>
    /// Returns a path that is relative to the ProcessedFolder (for final usage in controller).
    /// For instance, if absolutePath is "kahla-data/ImageCompressor/Compressed/.../file.jpg",
    /// and ProcessedFolder is "kahla-data/ImageCompressor", then the result might be
    /// "Compressed/.../file.jpg".
    /// </summary>
    public string GetPathRelativeToProcessed(string absolutePath)
    {
        return Path.GetRelativePath(options.ProcessedFolder, absolutePath);
    }

    private static string BuildDimensionSuffix(int width, int height)
    {
        if (width > 0 && height > 0)
        {
            return $"_w{width}_h{height}";
        }
        else if (width > 0)
        {
            return $"_w{width}";
        }
        else if (height > 0)
        {
            return $"_h{height}";
        }
        return string.Empty;
    }
}
