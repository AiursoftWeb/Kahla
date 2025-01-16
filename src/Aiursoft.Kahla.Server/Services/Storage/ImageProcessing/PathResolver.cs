namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Responsible for determining the correct physical paths for 
/// reading original files and writing processed files.
/// </summary>
public class PathResolver
{
    private readonly string _clearExifFolder;
    private readonly string _compressedFolder;
    private readonly string _workspaceFolder;

    public PathResolver(IConfiguration configuration)
    {
        var basePath = configuration["Storage:Path"] ??
                       throw new InvalidDataException("Missing config 'Storage:Path'!");
        _clearExifFolder = Path.Combine(basePath, "ClearExif");
        _compressedFolder = Path.Combine(basePath, "Compressed");
        _workspaceFolder = Path.Combine(basePath, "Workspace");

        if (!Directory.Exists(_workspaceFolder))
        {
            Directory.CreateDirectory(_workspaceFolder);
        }

        if (!Directory.Exists(_clearExifFolder))
        {
            Directory.CreateDirectory(_clearExifFolder);
        }

        if (!Directory.Exists(_compressedFolder))
        {
            Directory.CreateDirectory(_compressedFolder);
        }
    }

    private static string BuildDimensionSuffix(int width, int height)
    {
        if (width > 0 && height > 0)
        {
            return $"_w{width}_h{height}";
        }

        if (width > 0)
        {
            return $"_w{width}";
        }

        if (height > 0)
        {
            return $"_h{height}";
        }

        return string.Empty;
    }

    private string GetRelativePath(string file, string folder)
    {
        var pathUri = new Uri(file);
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            folder += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString()
            .Replace('/', Path.DirectorySeparatorChar));
    }

    public string GetClearedExifAbsolutePath(string sourceAbsolute)
    {
        var relativeToWorkspace = GetRelativePath(sourceAbsolute, _workspaceFolder);
        var targetFileLocation = Path.Combine(_clearExifFolder, relativeToWorkspace);
        var targetFolder = Path.GetDirectoryName(targetFileLocation);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder!);
        }

        return targetFileLocation;
    }

    public string GetCompressedAbsolutePath(string sourceAbsolute, int width, int height)
    {
        var relativeToWorkspace = GetRelativePath(sourceAbsolute, _workspaceFolder);
        var fileName = Path.GetFileNameWithoutExtension(relativeToWorkspace);
        var extension = Path.GetExtension(relativeToWorkspace);
        var folder = Path.GetDirectoryName(relativeToWorkspace) ?? string.Empty;

        var dimensionSuffix = BuildDimensionSuffix(width, height);
        var targetFolder = Path.Combine(_compressedFolder, folder);
        var targetFileName = $"{fileName}{dimensionSuffix}{extension}";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        return Path.Combine(targetFolder, targetFileName);
    }
}